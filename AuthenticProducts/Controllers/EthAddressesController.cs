using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AuthenticProducts.Models;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using AuthenticProducts.Models;

namespace AuthenticProducts.Controllers
{
    public class EthAddressesController : Controller
    {
        private AuthProductEntities db = new AuthProductEntities();
        string EthUrl = "https://ropsten.infura.io/v3/1878b2fa5df74a0b8f259ff2a11d417c";
        public string MasterEthPublicKey = "0x14f82ce727Dd884fd989e78CbB68db9bb823B0F0";
        string MasterEthPrivateKey = "84B8A84EE89B2126C1693C4159D08A689FD8CE2E8A23FE3F0D8EEAF1B9BA2D99";
        // GET: EthAddresses
        public ActionResult Index()
        {
            return View();
        }

        //Generate New Ethereum Address
        public EthAddress GenerateEthAddress()
        {
            EthAddress eta = new EthAddress();

            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var account = new Account(privateKey);
            eta.EthPublicKey = account.Address;
            eta.EthPrivateKey = privateKey;

            return eta;
        }

        //Sending Eth To Address with amount
        public bool TransferToAddress(string FromAddress, string ToAddress, float Amount, string PrivateKey)
        {
            return true;
        }

        public async Task<Models.Transaction> SendTransaction(string ToUserAddress, Decimal Amount, string UserPrivateKey = "" )
        {
            BigInteger gas = Web3.Convert.ToWei(3000000);
            BigInteger valueAmount = Web3.Convert.ToWei(4000000);
            BigInteger gasprice = Web3.Convert.ToWei(5000000000);
            Decimal EthAmount = UserControl.UserTypeID == 1 ? 0.0002m : 0.00015m;
            //Minimum gass cost is 0.000042 Ether, so send atleast 0.0002

            string FromPrivateKey = UserControl.UserTypeID == 2 ? UserPrivateKey : MasterEthPrivateKey ;
            string ToPublicKey = UserControl.UserTypeID == 2 ? MasterEthPublicKey : ToUserAddress;

            var account = new Account(FromPrivateKey);
            var web3 = new Web3(account, EthUrl);

            //TransactionInput ti = new TransactionInput();
            //ti.Data = TransactionData;
            //ti.From = account.Address;
            //ti.To = ToAddress;
            //ti.Value = web3.Eth HexBigInteger
            //var TR = await web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(ti,)

            var TR = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(ToPublicKey, EthAmount , 2);

            Models.Transaction trans = new Models.Transaction();
            trans.TransactionHash = TR.TransactionHash;
            trans.GasUsed = TR.GasUsed.Value.ToString();
            trans.BlockNumber = TR.BlockNumber.Value.ToString();
            trans.EthAmount = EthAmount;
            trans.FromAddress = account.Address;
            trans.ToAddress = ToUserAddress;

            return trans;
        }

        public async Task<Decimal> GetAccountBalance(string UserPublicKey = "")
        {
            string FromPublicKey = UserControl.UserTypeID == 2 ? UserPublicKey : MasterEthPublicKey;
            var web3 = new Web3(EthUrl);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(FromPublicKey);
            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<Models.Transaction> SendTransactionAndData(string ToUserAddress, Decimal Amount, string TransactionData = "", string UserPrivateKey = "")
        {
            string FromPrivateKey = UserControl.UserTypeID == 2 ? UserPrivateKey : MasterEthPrivateKey;
            string ToPublicKey = UserControl.UserTypeID == 2 ? MasterEthPublicKey : ToUserAddress;
            Models.Transaction trans = new Models.Transaction();

            var account = new Account(FromPrivateKey);
            var web3 = new Web3(account, EthUrl);
            var dataHex = TransactionData.ToHexUTF8();

            TransactionInput Tinput = new TransactionInput();

            Tinput.From = web3.TransactionManager.Account.Address;
            Tinput.To = ToPublicKey;
            Tinput.Value = new HexBigInteger(Web3.Convert.ToWei(Amount));
            Tinput.Data = dataHex.ToString();
            Tinput.Gas = await web3.TransactionManager.EstimateGasAsync(Tinput);

            var TransHash = await web3.TransactionManager.SendTransactionAndWaitForReceiptAsync(Tinput, new System.Threading.CancellationTokenSource());
            //.SendTransactionAsync(Tinput);
            //trans = await GetTransactionByHash(TransHash);

            trans.FromAddress = account.Address;
            trans.ToAddress = ToUserAddress;
            trans.TransactionHash = TransHash.TransactionHash;
            trans.GasUsed = TransHash.GasUsed.Value.ToString();
            trans.BlockNumber = TransHash.BlockNumber.Value.ToString();
            trans.InputDataText = TransactionData;
            trans.EthAmount = Amount;

            return trans;
        }

        public async Task<Models.Transaction> GetTransactionByHash(string TranHash)
        {
            var web3 = new Web3(EthUrl);
            Models.Transaction trans = new Models.Transaction();
            try
            {
            GetTransDetail:
                var transReceipt = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(TranHash);

                if (transReceipt.BlockNumber.Value == 0)
                {
                    System.Threading.Thread.Sleep(12000);
                    goto GetTransDetail;
                }

                trans.FromAddress = transReceipt.From;
                trans.ToAddress = transReceipt.To;
                trans.BlockNumber = transReceipt.BlockNumber.Value.ToString();
                trans.InputDataText = transReceipt.Input.HexToUTF8String();
                trans.InputDataHex = transReceipt.Input;
                trans.GasUsed = transReceipt.Gas.Value.ToString();
                trans.TransactionHash = transReceipt.TransactionHash;
                trans.EthAmount = Web3.Convert.FromWei(BigInteger.Parse(transReceipt.Value.ToString()));
            }
            catch (Exception ex)
            {
                trans.ErrorMessages = ex.Message;
            }

            return trans;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
