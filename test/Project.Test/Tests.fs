namespace Superfeed.Test

open System
open System.Threading
open Expecto
open Fake.Core
open StackExchange.Redis
open Superfeed.Core
open Superfeed.Core
open Superfeed.Core.QuoteAndTradeHandler
open Superfeed.Test
open Superfeed.Test.Listen
open Superfeed.Test.Listen
open Wombat

module Tests =

    type Result =
    | Empty
    | Clean
    | Dirty of int
    
    [<Tests>]
    let tests =
        testSequenced <|
            testList "SuperfeedTests" [
//                test "MultiThread Cache" {
//                    
//
//                    
//                    let cache = SuperfeedCache(1)
//                    let stop1 = false
//                    let stop2 = false
//                    
//                    let dValue = DirtyValue("test")
//                    let mutable i = 0
//                    let mutable values = []
//                       
//                    let loopCnt = 2000000   
//                    let threadFn2() =
//                        while i < loopCnt do
//                            Interlocked.Increment(&i) |> ignore
//                            dValue.UpdateValue(i.ToString())
//                            cache.Enqueue 0 (dValue)
//                    
//                    let threadFn1() =
//                        while not stop1 do
//                            match cache.TryTake (0,10) with
//                            | ValueSome(key,ValueSome(value)) -> values <- Result.Dirty( Int32.Parse(value)) :: values
//                            | ValueSome(key,ValueNone) -> values <- Result.Clean :: values
//                            | ValueNone -> values <- Result.Empty :: values
//                    
//                   
//                    let t1 = Thread(ThreadStart(threadFn1))
//                    let t2 = Thread(ThreadStart(threadFn2))
//                    
//                    t1.Start()
//                    t2.Start()
//                    
//                    t2.Join()
//                    
//                    Thread.Sleep(5000)
//                    
//                    let pairs = values |> List.rev |> List.windowed 2 |> List.map (fun (a :: (b :: [])) -> (a,b)) |> List.toArray
//                    let dvalues = values |> List.choose (function | Result.Dirty(x) -> Some(x) | _ -> None) |> List.toArray
//                    
//                    for (r1,r2) in pairs do
//                        match r1,r2 with
//                        | Result.Empty, Result.Empty -> failwith "should never go from empty to empty"
//                        | Result.Empty, Result.Clean -> failwith "should never go from empty to clean"
//                        | _,_ -> ignore ()
//                   
//                    Expect.isDescending dvalues "order maintained"     
//                        
//                    
//                }
                
                test "Sanity Check: Msg Count" {
                    let pubTask = Tools.RunPublisher "test_1.json"
        
                    let testOptions =
                        {   TestSymbols = TestSymbolsOpt.Symbols(["GOOG"])
                            Source = "NASDAQ"
                            Transport = "test_subscribe"
                            LogLevel = 0
                            Env = "DEBUG" }
                    
                    let count = ref 0
                    
                    let handleFn (msg,typ) =
                        Interlocked.Increment(count) |> ignore
                    
                    let builder dict (symbol,threadIndex) =
                        TestHandler(dict,handleFn) :> ISuperfeedMessageHandler
                        
                    using (new TestListener(testOptions)) (fun testListener ->
                        testListener.Start(builder)
                        let pubResult = pubTask |> Async.AwaitTask |> Async.RunSynchronously
                        Expect.equal pubResult.ExitCode 0 "Publisher Exit Code is 0"
                        )

                    Expect.equal count.Value 37 "37 messages received"
                }
                
                test "Sanity Check 2: Final State" {
        
                    let pubTask = Tools.RunPublisher "test_1.json"
        
                    let testOptions =
                        {   TestSymbols = TestSymbolsOpt.Symbols(["GOOG"])
                            Source = "NASDAQ"
                            Transport = "test_subscribe"
                            LogLevel = 0
                            Env = "DEBUG" }
                    
                    let testWriter = TestWriter()
                    
                    let builder dict (symbol,threadIndex) =
                        QuoteAndTradeHandler(symbol,testWriter.Cache1,threadIndex,dict) :> ISuperfeedMessageHandler
                        
                    using (new TestListener(testOptions)) (fun testListener ->
                    
                        testListener.Start(builder)
                            
                        let pubResult = pubTask |> Async.AwaitTask |> Async.RunSynchronously
                        Expect.equal pubResult.ExitCode 0 "Publisher Exit Code is 0"
                        
                        while (testListener.Manager.GetSubscription("GOOG").Value.MessagesReceivedCount < 37UL) do
                            Thread.Sleep(10)
                        
                    )
//                    for KeyValue(k,v) in testWriter.Dictionary do
//                        printfn " Expect.equal testWriter.Dictionary.[ \"%s\" ] \"%s\" \"\" " k v
                    let t = testWriter.Consume()
                    testWriter.CTS.CancelAfter(50)
                    t.Join()
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.bid_price" ] "1558.53" "bid_price \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.last_timestamp" ] "1595272716" "last_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.ask_timestamp" ] "1595272717" "ask_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.last_price" ] "1558.53" "last_price \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.ask_size" ] "100" "ask_size \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.last_size" ] "1" "last_size \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.bid_size" ] "100" "bid_size \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.bid_timestamp" ] "1595272715" "bid_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.GOOG.ask_price" ] "1559.62" "ask_price \u2713"

                }
    
                test "Sanity Check 3: Final State (MSFT)" {
        
                    let pubTask = Tools.RunPublisher "test_3.json"
        
                    let testOptions =
                        {   TestSymbols = TestSymbolsOpt.Symbols(["MSFT"])
                            Source = "NASDAQ"
                            Transport = "test_subscribe"
                            LogLevel = 0
                            Env = "DEBUG" }
                    
                    let testWriter = TestWriter()
                    
                    let builder dict (symbol,threadIndex) =
                        QuoteAndTradeHandler(symbol,testWriter.Cache1,threadIndex,dict) :> ISuperfeedMessageHandler
                        
                    using (new TestListener(testOptions)) (fun testListener ->
                    
                        testListener.Start(builder)
                        
                        let consumeThread = testWriter.Consume()
                            
                        let pubResult = pubTask |> Async.AwaitTask |> Async.RunSynchronously
                        Expect.equal pubResult.ExitCode 0 "Publisher Exit Code is 0"
                        

                        while testListener.Manager.GetSubscription("MSFT").Value.MessagesReceivedCount < 1467UL do
                            Thread.Sleep(10)


                        Thread.Sleep(50)
                        testWriter.CTS.Cancel()
                        consumeThread.Join()
                    )
                    

                    
                    
                    
                    
                    printfn "Count %i" testWriter.Count
                    
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.bid_timestamp" ] "1597934634" "bid_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.last_price" ] "211.165" "last_price \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.ask_timestamp" ] "1597934634" "ask_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.bid_price" ] "211.15" "bid_price \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.bid_size" ] "100" "bid_size \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.ask_size" ] "200" "ask_size \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.last_timestamp" ] "1597934634" "last_timestamp \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.ask_price" ] "211.18" "ask_price \u2713"
                    Expect.equal testWriter.Dictionary.[ "prices.MSFT.last_size" ] "50" "last_size \u2713"
                
                }
            
            ]