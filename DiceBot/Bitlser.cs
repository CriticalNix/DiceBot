﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceBot
{
    class Bitsler:DiceSite
    {
        bool IsBitsler = false;
        string accesstoken = "";
        DateTime LastSeedReset = new DateTime();
        
        string username = "";
        long uid = 0;
        DateTime lastupdate = new DateTime();
        HttpClient Client;// = new HttpClient { BaseAddress = new Uri("https://api.primedice.com/api/") };
        public static string[] sCurrencies = new string[] { "btc","ltc","doge" };
        HttpClientHandler ClientHandlr;
        public Bitsler(cDiceBot Parent)
        {
            Currencies = new string[] { "btc","ltc","doge" };
            maxRoll = 99.999;
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
            AutoLogin = true;
            BetURL = "https://api.primedice.com/bets/";
            /*Thread t = new Thread(GetBalanceThread);
            t.Start();*/
            this.Parent = Parent;
            Name = "Bitsler";
            Tip =false;
            TipUsingName = true;
            //Thread tChat = new Thread(GetMessagesThread);
            //tChat.Start();
            SiteURL = "https://www.bitsler.com/?ref=seuntjie";
            register = false;
            
        }
        void GetBalanceThread()
        {
            while (IsBitsler)
            {
                if ((DateTime.Now - lastupdate).TotalSeconds > 15)
                {
                    lastupdate = DateTime.Now;
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                    pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                    FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                    string sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                    bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                    switch (Currency.ToLower())
                    {
                        case "btc": balance = bsstatsbase._return.btc_balance;
                            profit = bsstatsbase._return.btc_profit;
                            wagered = bsstatsbase._return.btc_wagered;break;
                        case "ltc": balance = bsstatsbase._return.ltc_balance;
                            profit = bsstatsbase._return.ltc_profit;
                            wagered = bsstatsbase._return.ltc_wagered; break;
                        case "doge": balance = bsstatsbase._return.doge_balance;
                            profit = bsstatsbase._return.doge_profit;
                            wagered = bsstatsbase._return.doge_wagered; break;
                    }
                    bets = int.Parse(bsstatsbase._return.bets);
                    wins = int.Parse(bsstatsbase._return.wins);
                    losses = int.Parse(bsstatsbase._return.losses);
                    
                    Parent.updateBalance(balance);
                    Parent.updateBets(bets);
                    Parent.updateLosses(losses);
                    Parent.updateProfit(profit);
                    Parent.updateWagered(wagered);
                    Parent.updateWins(wins);
                }
                Thread.Sleep(1000);
            }
        }

        protected override void CurrencyChanged()
        {
            base.CurrencyChanged();
            lastupdate = DateTime.Now;
            if (accesstoken != "" && IsBitsler)
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                switch (Currency.ToLower())
                {
                    case "btc": balance = bsstatsbase._return.btc_balance;
                        profit = bsstatsbase._return.btc_profit;
                        wagered = bsstatsbase._return.btc_wagered; break;
                    case "ltc": balance = bsstatsbase._return.ltc_balance;
                        profit = bsstatsbase._return.ltc_profit;
                        wagered = bsstatsbase._return.ltc_wagered; break;
                    case "doge": balance = bsstatsbase._return.doge_balance;
                        profit = bsstatsbase._return.doge_profit;
                        wagered = bsstatsbase._return.doge_wagered; break;
                } 
                bets = int.Parse(bsstatsbase._return.bets);
                wins = int.Parse(bsstatsbase._return.wins);
                losses = int.Parse(bsstatsbase._return.losses);
                
                Parent.updateBalance(balance);
                Parent.updateBets(bets);
                Parent.updateLosses(losses);
                Parent.updateProfit(profit);
                Parent.updateWagered(wagered);
                Parent.updateWins(wins);
            }
        }

        void PlaceBetThread(object BetObj)
        {
            try
            {
                PlaceBetObj tmpob = BetObj as PlaceBetObj;
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                /*access_token
type:dice
amount:0.00000001
condition:< or >
game:49.5
devise:btc*/
                pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                pairs.Add(new KeyValuePair<string, string>("type", "dice"));
                pairs.Add(new KeyValuePair<string, string>("amount", tmpob.Amount.ToString("0.00000000")));
                pairs.Add(new KeyValuePair<string, string>("condition", tmpob.High?">":"<"));
                pairs.Add(new KeyValuePair<string, string>("game", !tmpob.High ? tmpob.Chance.ToString("0.00") : ( maxRoll -tmpob.Chance).ToString("0.00")));
                pairs.Add(new KeyValuePair<string, string>("devise", Currency));
                pairs.Add(new KeyValuePair<string, string>("key", "seuntjiebot"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("bet", Content).Result.Content.ReadAsStringAsync().Result;
                bsBetBase bsbase = json.JsonDeserialize<bsBetBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success=="true")
                        {
                            balance = double.Parse(bsbase._return.new_balance, System.Globalization.NumberFormatInfo.InvariantInfo);
                            Bet tmp = bsbase._return.ToBet();
                            profit += (double)tmp.Profit;
                            wagered += (double)tmp.Amount;
                            bool win = false;
                            if ((tmp.Roll > 99.99m - tmp.Chance && tmp.high) || (tmp.Roll < tmp.Chance && !tmp.high))
                            {
                                win = true;
                            }
                            //set win
                            if (win)
                                wins++;
                            else
                                losses++;
                            bets++;
                            FinishedBet(tmp);
                            return;
                        }
                Parent.updateStatus("An Unknown error has ocurred.");
                //

            }
            catch
            {
                Parent.updateStatus("An Unknown error has ocurred.");
            }
        }
        protected override void internalPlaceBet(bool High, double amount, double chance)
        {
            System.Threading.Thread tBetThread = new Thread(new ParameterizedThreadStart(PlaceBetThread));
            tBetThread.Start(new PlaceBetObj(High, amount, chance));
        }

        public override void ResetSeed()
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed)
        {
            throw new NotImplementedException();
        }

        protected override bool internalWithdraw(double Amount, string Address)
        {
            throw new NotImplementedException();
        }

        public override void Login(string Username, string Password, string twofa)
        {
            ClientHandlr = new HttpClientHandler { UseCookies = true, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip, Proxy = this.Prox, UseProxy = Prox != null };
            Client = new HttpClient(ClientHandlr) { BaseAddress = new Uri("https://www.bitsler.com/api/") };
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            try
            {

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("username", Username));
                pairs.Add(new KeyValuePair<string, string>("password", Password));
                //if (!string.IsNullOrWhiteSpace(twofa))
                {
                    pairs.Add(new KeyValuePair<string, string>("two_factor", twofa));
                }

                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                string sEmitResponse = Client.PostAsync("login", Content).Result.Content.ReadAsStringAsync().Result;
                
                //getuserstats 
                bsloginbase bsbase = json.JsonDeserialize<bsloginbase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                
                if (bsbase!=null)
                    if (bsbase._return!=null)
                        if (bsbase._return.success=="true")
                        {
                            accesstoken = bsbase._return.access_token;
                            IsBitsler = true;
                            lastupdate = DateTime.Now;
                            
                            pairs = new List<KeyValuePair<string, string>>();
                            pairs.Add(new KeyValuePair<string, string>("access_token", accesstoken));
                            Content = new FormUrlEncodedContent(pairs);
                            sEmitResponse = Client.PostAsync("getuserstats", Content).Result.Content.ReadAsStringAsync().Result;
                            bsStatsBase bsstatsbase = json.JsonDeserialize<bsStatsBase>(sEmitResponse.Replace("\"return\":", "\"_return\":"));
                            switch (Currency.ToLower())
                            {
                                case "btc": balance = bsstatsbase._return.btc_balance;
                                    profit = bsstatsbase._return.btc_profit;
                                    wagered = bsstatsbase._return.btc_wagered; break;
                                case "ltc": balance = bsstatsbase._return.ltc_balance;
                                    profit = bsstatsbase._return.ltc_profit;
                                    wagered = bsstatsbase._return.ltc_wagered; break;
                                case "doge": balance = bsstatsbase._return.doge_balance;
                                    profit = bsstatsbase._return.doge_profit;
                                    wagered = bsstatsbase._return.doge_wagered; break;
                            } 
                            bets = int.Parse(bsstatsbase._return.bets);
                            wins = int.Parse(bsstatsbase._return.wins);
                            losses = int.Parse(bsstatsbase._return.losses);
                            
                            Parent.updateBalance(balance);
                            Parent.updateBets(bets);
                            Parent.updateLosses(losses);
                            Parent.updateProfit(profit);
                            Parent.updateWagered(wagered);
                            Parent.updateWins(wins);
                            IsBitsler = true;
                            Thread t = new Thread(GetBalanceThread);
                            t.Start();
                            finishedlogin(true);
                            return;
                        }

            }
            catch 
            {

            }
            finishedlogin(false);
        }

        public override bool Register(string username, string password)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet()
        {
            return true;
        }

        public override void Disconnect()
        {
            IsBitsler = false;
        }

        public override void GetSeed(long BetID)
        {
            throw new NotImplementedException();
        }

        public override void SendChatMessage(string Message)
        {
            throw new NotImplementedException();
        }

        public static double sGetLucky(string server, string client, int nonce)
        {
            SHA1 betgenerator = SHA1.Create();
            string Seed = server + "-" + client + "-" + nonce;
            byte[] serverb = new byte[Seed.Length];

            for (int i = 0; i < Seed.Length; i++)
            {
                serverb[i] = Convert.ToByte(Seed[i]);
            }
            double Lucky = 0;
            do
            {
                serverb = betgenerator.ComputeHash(serverb.ToArray());
                StringBuilder hex = new StringBuilder(serverb.Length * 2);
                foreach (byte b in serverb)
                    hex.AppendFormat("{0:x2}", b);

                string s = hex.ToString().Substring(0, 8);
                Lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            } while (Lucky > 4294960000);
            Lucky = (Lucky % 10000.0) / 100.0;
            if (Lucky < 0)
                return -Lucky;
            return Lucky;
        }

        public override double GetLucky(string server, string client, int nonce)
        {
            
            SHA1 betgenerator = SHA1.Create();
            string Seed = server+"-"+client+"-"+nonce;
            byte[] serverb = new byte[Seed.Length];

            for (int i = 0; i < Seed.Length; i++)
            {
                serverb[i] = Convert.ToByte(Seed[i]);
            }
            double Lucky = 0;
            do
            {
                serverb = betgenerator.ComputeHash(serverb.ToArray());
                StringBuilder hex = new StringBuilder(serverb.Length * 2);
                foreach (byte b in serverb)
                    hex.AppendFormat("{0:x2}", b);

                string s = hex.ToString().Substring(0, 8);
                Lucky = long.Parse(s, System.Globalization.NumberStyles.HexNumber);
            } while (Lucky > 4294960000);
            Lucky = (Lucky % 10000.0) / 100.0;
            if (Lucky < 0)
                return -Lucky;
            return Lucky;
            /*
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }

            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }*/
            return 0;
        }
    }

    public class bsLogin
    {
        public string success { get; set; }
        public string access_token { get; set; }
    }
    public class bsloginbase
    {
        public bsLogin _return { get; set; }
    }
    //"{\"return\":{\"success\":\"true\",\"balance\":1.0e-5,\"wagered\":0,\"profit\":0,\"bets\":\"0\",\"wins\":\"0\",\"losses\":\"0\"}}"
    public class bsStats
    {
        public string success { get; set; }
        public double btc_balance { get; set; }
        public double btc_wagered { get; set; }
        public double btc_profit { get; set; }
        public string bets { get; set; }
        public double ltc_balance { get; set; }
        public double ltc_wagered { get; set; }
        public double ltc_profit { get; set; }

        public double doge_balance { get; set; }
        public double doge_wagered { get; set; }
        public double doge_profit { get; set; }
        
        public string wins { get; set; }
        public string losses { get; set; }
    }
    public class bsStatsBase
    {
        public bsStats _return { get; set; }
    }
    public class bsBetBase
    {
        public bsBet _return { get; set; }
    }
    public class bsBet
    {
        public string success { get; set; }
        public string username { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string devise { get; set; }
        public long ts { get; set; }
        public string time { get; set; }
        public string amount { get; set; }
        public double roll_number { get; set; }
        public string condition { get; set; }
        public string game { get; set; }
        public double payout { get; set; }
        public string winning_chance { get; set; }
        public string amount_return { get; set; }
        public string new_balance { get; set; }
        public string _event { get; set; }

        public Bet ToBet()
        {
            Bet tmp = new Bet
            {
                Amount = decimal.Parse(amount, System.Globalization.NumberFormatInfo.InvariantInfo),
                date = json.ToDateTime2(ts.ToString()),
                Id = decimal.Parse(id, System.Globalization.CultureInfo.InvariantCulture),
                Profit = decimal.Parse(amount_return, System.Globalization.NumberFormatInfo.InvariantInfo),
                Roll = (decimal)roll_number,
                high = condition == ">",
                Chance = decimal.Parse(winning_chance, System.Globalization.NumberFormatInfo.InvariantInfo),
                nonce = -1,
                serverhash = "",
                clientseed = ""                
            };
            return tmp;
        }
    }
}