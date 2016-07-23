﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Net.Http;

namespace DiceBot
{
    class dice999:DiceSite
    {

        string sessionCookie = "";
        Random r = new Random();
        long uid = 0;
        
        bool isD999 = false;
        
        public static string[] cCurrencies =new string[] { "btc","doge","ltc","eth" };
        HttpClientHandler ClientHandlr;
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
        public dice999(cDiceBot Parent, bool doge999)
        {
            this.doge999 = doge999;
            maxRoll = 99.9999;
            this.Parent = Parent;
            AutoInvest = false;
            AutoWithdraw = true;
            edge = 0.1m;
            ChangeSeed = false;
            AutoLogin = false;
            if (doge999)
                BetURL = "https://www.999doge.com/Bets/?b=";
            else
            BetURL = "https://www.999dice.com/Bets/?b=";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "999Dice";
            Tip = false;
            TipUsingName = true;
            Currency = "btc";
            Currencies = cCurrencies;
            /*Thread tChat = new Thread(GetMessagesThread);
            tChat.Start();*/
            if (doge999)
                SiteURL = "https://www.999doge.com/?20073598";
            else
            SiteURL = "https://www.999dice.com/?20073598";
        }

        protected override void CurrencyChanged()
        {
            Lastbalance = DateTime.Now.AddMinutes(-2);
            GetBalance();
            GetDepositAddress();
        }
        DateTime Lastbalance = DateTime.Now;
        void GetBalanceThread()
        {
            while (isD999)
            {
                if (sessionCookie!="" && sessionCookie!=null && (DateTime.Now-Lastbalance).TotalSeconds>=60)
                {
                     GetBalance();

                }
                Thread.Sleep(1100);
            }
        }

        void GetBalance()
        {
            if (sessionCookie != "" && sessionCookie != null && (DateTime.Now - Lastbalance).TotalSeconds>60)
            {
                Lastbalance = DateTime.Now;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "GetBalance"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
                
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            GetBalance();
                            return;
                        }
                    }
                }
                try
                {
                    balance = (double)json.JsonDeserialize<d999Login>(responseData).Balance / 100000000.0;
                    if (balance != 0)
                    {
                        Parent.updateBalance((decimal)balance);
                    }
                    
                }
                catch
                {

                }
            }
        }
        int retrycount = 0;
        string next = "";
        void PlaceBetThread(object _High)
        {
            
            string err = "";
            try
            {
                PlaceBetObj tmp9 = _High as PlaceBetObj;

                bool High = tmp9.High;
                double amount = tmp9.Amount;
                //double chance = tmp9.Chance;

                Parent.updateStatus(string.Format("Betting: {0:0.00000000} at {1:0.00000000} {2}", amount, tmp9.Chance, High ? "High" : "Low"));

                double chance = (999999.0) * (tmp9.Chance / 100.0);
                //HttpWebResponse EmitResponse;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                if (next == "" && next!=null)
                {


                    
                    pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("a", "GetServerSeedHash"));
                    pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                    
                   Content = new FormUrlEncodedContent(pairs);
                     responseData = "";
                    using (var response = Client.PostAsync("", Content))
                    {
                        try
                        {
                            responseData = response.Result.Content.ReadAsStringAsync().Result;
                        }
                        catch (AggregateException e)
                        {
                            if (e.InnerException.Message.Contains("ssl"))
                            {
                                PlaceBetThread(High);
                                return;
                            }
                        }
                    }
                    if (responseData.Contains("error"))
                    {
                        if (retrycount++ < 3)
                        {

                            Thread.Sleep(200);
                            PlaceBetThread(High);
                            return;
                        }
                        else
                            throw new Exception();
                    }
                    string Hash = next =  json.JsonDeserialize<d999Hash>(responseData).Hash;
                }
                string ClientSeed = r.Next(0, int.MaxValue).ToString();
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "PlaceBet"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("PayIn", ((long)(amount * 100000000.0)).ToString("0",System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("Low", (High ? 999999 - (int)chance : 0).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("High", (High ? 999999 : (int)chance).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
                pairs.Add(new KeyValuePair<string, string>("ClientSeed", ClientSeed));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
                pairs.Add(new KeyValuePair<string, string>("ProtocolVersion", "2"));

                Content = new FormUrlEncodedContent(pairs);
                string tmps = Content.ReadAsStringAsync().Result;
                
                responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                        
                    }
                    catch (AggregateException e)
                    {
                        Parent.DumpLog(e.InnerException.Message, 0);
                        if (retrycount++ < 3)
                        {
                            PlaceBetThread(High);
                            return;
                        }
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            PlaceBetThread(High);
                            return;
                        }
                        else
                        {
                            Parent.updateStatus("An error has occurred");
                        }
                    }
                }
                
                
                d999Bet tmpBet = json.JsonDeserialize<d999Bet>(responseData);
                
                if (amount>=21)
                {

                }
                if (tmpBet.ChanceTooHigh == 1 || tmpBet.ChanceTooLow == 1 | tmpBet.InsufficientFunds == 1 || tmpBet.MaxPayoutExceeded == 1 || tmpBet.NoPossibleProfit == 1)
                {
                    if (tmpBet.ChanceTooHigh == 1)
                        err = "Chance too high";
                    if (tmpBet.ChanceTooLow == 1)
                        err += "Chance too Low";
                    if (tmpBet.InsufficientFunds == 1)
                        err += "Insufficient Funds";
                    if (tmpBet.MaxPayoutExceeded == 1)
                        err += "Max Payout Exceeded";
                    if (tmpBet.NoPossibleProfit == 1)
                        err += "No Possible Profit";
                    throw new Exception();
                }
                else if (tmpBet.BetId == 0)
                {
                    throw new Exception();
                }
                else
                {
                    balance = (double)tmpBet.StartingBalance / 100000000.0 - (amount) + ((double)tmpBet.PayOut / 100000000.0);

                    profit += -(amount) + (double)(tmpBet.PayOut / 100000000m);
                    Bet tmp = new Bet();
                    tmp.Amount = (decimal)amount;
                    tmp.BetDate = DateTime.Now.ToString(); ;
                    tmp.Chance = ((decimal)chance * 100m) / 999999m;
                    tmp.clientseed = ClientSeed;
                    tmp.Currency = Currency;
                    tmp.high = High;
                    tmp.Id = tmpBet.BetId;
                    tmp.nonce = 0;
                    tmp.Profit = ((decimal)tmpBet.PayOut / 100000000m) - ((decimal)amount);
                    tmp.Roll = tmpBet.Secret / 10000m;
                    tmp.serverhash = next;
                    tmp.serverseed = tmpBet.ServerSeed;
                    tmp.uid = (int)uid;
                    tmp.UserName = "";

                    bool win = false;
                    if ((tmp.Roll > 99.99m - tmp.Chance && High) || (tmp.Roll < tmp.Chance && !High))
                    {
                        win = true;
                    }
                    if (win)
                        wins++;
                    else
                        losses++;
                    Wagered += tmp.Amount;
                    bets++;
                    

                    sqlite_helper.InsertSeed(tmp.serverhash, tmp.serverseed);
                    next = tmpBet.Next;
                    retrycount = 0;
                    FinishedBet(tmp);
                }
            }
            catch
            {
                if (err != "")
                    Parent.updateStatus(err);
                else
                    Parent.updateStatus("Something went wrong! stopped betting");
            }
        }

        protected override void internalPlaceBet(bool High, double amount, double chance)
        {
            this.High = High;
            Thread t = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            t.Start(new PlaceBetObj(High, amount, chance));
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

       
       

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            isD999 = false;
        }

        public override void GetSeed(long BetID)
        {
            
        }

        public override void SendChatMessage(string Message)
        {
            
        }

        public override void Donate(double Amount)
        {
            internalWithdraw(Amount, "1BoHcFQsUSot7jkHJcZMh1iUda3tEjzuBW");
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "Withdraw"));
            pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
            pairs.Add(new KeyValuePair<string, string>("Currency", Currency));
            pairs.Add(new KeyValuePair<string, string>("Amount", (Amount*100000000).ToString(System.Globalization.NumberFormatInfo.InvariantInfo)));
            pairs.Add(new KeyValuePair<string, string>("Address", Address));

            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        return internalWithdraw(Amount , Address);
                    }
                }
            }
            
            return true;
        }

        bool doge999 = false;
        decimal Wagered = 0;
        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            if (doge999)
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999doge.com/api/web.aspx") };
            else
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "Login"));
            pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
            if (twofa!="" && twofa!=null)
            pairs.Add(new KeyValuePair<string, string>("Totp", twofa));

            pairs.Add(new KeyValuePair<string, string>("Username", Username));
            pairs.Add(new KeyValuePair<string, string>("Password", Password));

            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        Login(Username, Password, twofa);
                        return;
                    }
                }
            }
            
            d999Login tmpU = json.JsonDeserialize<d999Login>(responseData);
            if (tmpU.SessionCookie!="" && tmpU.SessionCookie!=null)
            {
                Lastbalance = DateTime.Now;
                sessionCookie = tmpU.SessionCookie;
                balance = (double)tmpU.Balance / 100000000.0;
                profit = (double)tmpU.Profit/100000000.0;
                Wagered = tmpU.Wagered/100000000m;
                bets = (int)tmpU.BetCount;
                wins = (int)tmpU.BetWinCount;
                losses = (int)tmpU.BetLoseCount;
                GetBalance();
                Parent.updateBalance((decimal)(balance));
                Parent.updateBets(tmpU.BetCount);
                Parent.updateLosses(tmpU.BetLoseCount);
                Parent.updateProfit(profit);
                Parent.updateWagered(Wagered);
                Parent.updateWins(tmpU.BetWinCount);
                Lastbalance = DateTime.Now.AddMinutes(-2);
                GetBalance();
                try
                {
                    Parent.updateDeposit(tmpU.DepositAddress);
                }
                catch { }
                uid = tmpU.Accountid;
            }      
            else
            {
                
            }
            if (sessionCookie!="")
            {
                isD999 = true;
                Thread t = new Thread(GetBalanceThread);
                t.Start();
                
            }
            finishedlogin(sessionCookie != "");
        }
        public override bool Register(string username, string password)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression= DecompressionMethods.Deflate| DecompressionMethods.GZip, Proxy= this.Prox, UseProxy=Prox!=null };;
            if (doge999)
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999doge.com/api/web.aspx") };
            else
                Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.999dice.com/api/web.aspx") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            pairs.Add(new KeyValuePair<string, string>("a", "CreateAccount"));
            pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
            FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
            string responseData = "";
            using (var response = Client.PostAsync("", Content))
            {
                try
                {
                    responseData = response.Result.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.Message.Contains("ssl"))
                    {
                        return Register(username, password);
                        
                    }
                }
            }
            
            d999Register tmp = json.JsonDeserialize<d999Register>(responseData);
            if (tmp.SessionCookie!="" && tmp.SessionCookie!=null)
            {
                sessionCookie = tmp.SessionCookie;
                pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "CreateUser"));
                pairs.Add(new KeyValuePair<string, string>("key", "7a3ada10cb804ec695cda315db6b8789"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Username", username));
                pairs.Add(new KeyValuePair<string, string>("Password", password));
                Content = new FormUrlEncodedContent(pairs);
                responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            return Register(username, password);

                        }
                    }
                }
                
                Parent.updateBalance((decimal)(balance));
                 Parent.updateBets(0);
                 Parent.updateLosses(0);
                 Parent.updateProfit(0m);
                 Parent.updateWagered(0m);
                 Parent.updateWins(0);
                 Parent.updateDeposit(tmp.DepositAddress);
                 uid = tmp.Accountid;
            }
            else
            {

            } 
            if (sessionCookie != "")
            {
                isD999 = true;
                Thread t = new Thread(GetBalanceThread);
                t.Start();

            }
            return sessionCookie != "" && sessionCookie != null;
        }

        public void GetDepositAddress()
        {
            if (sessionCookie != "" && sessionCookie != null)
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("a", "GetDepositAddress"));
                pairs.Add(new KeyValuePair<string, string>("s", sessionCookie));
                pairs.Add(new KeyValuePair<string, string>("Currency", Currency));

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string responseData = "";
                using (var response = Client.PostAsync("", Content))
                {
                    try
                    {
                        responseData = response.Result.Content.ReadAsStringAsync().Result;
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException.Message.Contains("ssl"))
                        {
                            GetDepositAddress();
                            return;

                        }
                    }
                }
                
                
                d999deposit tmp = json.JsonDeserialize<d999deposit>(responseData);
                Parent.updateDeposit(tmp.Address);
            }
        }

        public override double GetLucky(string serverSeed, string clientSeed, int nonce)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
                .Range(0, s.Length / 2)
                .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
                .ToArray();
            byte[] server = strtobytes(serverSeed);
            byte[] client = BitConverter.GetBytes(int.Parse(clientSeed)).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(nonce).Reverse().ToArray();
            byte[] data = server.Concat(client).Concat(num).ToArray();
            using (SHA512 sha512 = new SHA512Managed())
            {
               /* if (serverhash != null)
                    using (SHA256 sha256 = new SHA256Managed())
                        if (!sha256.ComputeHash(server).SequenceEqual(serverhash))
                            throw new Exception("Server seed hash does not match server seed");*/
                byte[] hash = sha512.ComputeHash(sha512.ComputeHash(data));
                while (true)
                {
                    for (int x = 0; x <= 61; x += 3)
                    {
                        long result = (hash[x] << 16) | (hash[x + 1] << 8) | hash[x + 2];
                        if (result < 16000000)
                        {
                            return (result % 1000000)/10000.0;
                        }
                    }
                    hash = sha512.ComputeHash(hash);
                }
            }
            
        }
        public static double sGetLucky(string serverSeed, string clientSeed, int betNumber/*, long betResult*/, string serverSeedHash = null)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
                .Range(0, s.Length / 2)
                .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
                .ToArray();
            byte[] server = strtobytes(serverSeed);
            byte[] client = BitConverter.GetBytes(int.Parse(clientSeed)).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(betNumber).Reverse().ToArray();
            byte[] serverhash = serverSeedHash == null ? null : strtobytes(serverSeedHash);
            byte[] data = server.Concat(client).Concat(num).ToArray();
            using (SHA512 sha512 = new SHA512Managed())
            {
                if (serverhash != null)
                    using (SHA256 sha256 = new SHA256Managed())
                        if (!sha256.ComputeHash(server).SequenceEqual(serverhash))
                        {
                            return -1;
                        }
                byte[] hash = sha512.ComputeHash(sha512.ComputeHash(data));
                while (true)
                {
                    for (int x = 0; x <= 61; x += 3)
                    {
                        long result = (hash[x] << 16) | (hash[x + 1] << 8) | hash[x + 2];
                        if (result < 16000000)
                        {
                            return (result % 1000000)/10000.0;
                        }
                    }
                    hash = sha512.ComputeHash(hash);
                }
            }
        }
    }

    public class d999Register
    {
        public string AccountCookie { get; set; }
        public string SessionCookie { get; set; }
        public long Accountid { get; set; }
        public int MaxBetBatchSize { get; set; }
        public string ClientSeed { get; set; }
        public string DepositAddress { get; set; }
    }

    public class d999Login:d999Register
    {
        public decimal Balance { get; set; }
        public string Email { get; set; }
        public string EmergenctAddress { get; set; }
        public long BetCount { get; set; }
        public long BetWinCount { get; set; }
        public long BetLoseCount { get { return BetCount - BetWinCount; } }
        public decimal BetPayIn { get; set; }
        public decimal BetPayOut { get; set; }
        public decimal Profit { get {return BetPayIn+BetPayOut;} }
        public decimal Wagered { get { return BetPayOut - BetPayIn; } }
    }

    public class d999Hash
    {
        public string Hash { get; set; }
    }
    public class d999deposit
    {
        public string Address { get; set; }
    }
    public class d999Bet
    {
        public long BetId { get; set; }
        public decimal PayOut { get; set; }
        public decimal Secret { get; set; }
        public decimal StartingBalance { get; set; }
        public string ServerSeed { get; set; }
        public string Next { get; set; }

        public int ChanceTooHigh { get; set; }
        public int ChanceTooLow { get; set; }
        public int InsufficientFunds { get; set; }
        public int NoPossibleProfit { get; set; }
        public int MaxPayoutExceeded { get; set; }
    }

    
    
}
