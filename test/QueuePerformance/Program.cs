using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CuteAnt.Collections;

namespace QueuePerformance
{
  class Program
  {
    static void Main(string[] args)
    {
      //var deque1 = new Deque<int>(new int[] { 1, 2, 3, 4 });
      //for (var idx = 5; idx <= 10; idx++)
      //{
      //    deque1.AddToFront(idx);
      //}

      //deque1.ForEach(_ => Console.WriteLine(_));
      ////deque1.Reverse(_ => Console.WriteLine(_));

      //Console.WriteLine("Queue");
      //var queue1 = new Queue<int>(new int[] { 1, 2, 3, 4 });
      //for (var idx = 5; idx <= 10; idx++)
      //{
      //    queue1.Enqueue(idx);
      //}
      //foreach (var item in queue1)
      //{
      //    Console.WriteLine(item);
      //}

      //Console.WriteLine("Stack");
      //var stack1 = new Stack<int>(new int[] { 1, 2, 3, 4 });
      //for (var idx = 5; idx <= 10; idx++)
      //{
      //    stack1.Push(idx);
      //}
      //foreach (var item in stack1)
      //{
      //    Console.WriteLine(item);
      //}

      //Console.WriteLine("LinkedList");
      //var linkedlist = new LinkedList<int>(new int[] { 1, 2, 3, 4 });
      //for (var idx = 5; idx <= 10; idx++)
      //{
      //    linkedlist.AddFirst(idx);
      //}
      //foreach (var item in linkedlist)
      //{
      //    Console.WriteLine(item);
      //}

      //Console.WriteLine("按任意键继续");
      //Console.ReadKey();
      //return;

      Random rand = new Random();
      Stopwatch sw = new Stopwatch();
      Queue<int> queue = new Queue<int>(8);
      Stack<int> stack = new Stack<int>(8);
      StackX<int> stackX = new StackX<int>(8);
      QueueX<int> queueX = new QueueX<int>(8);
      var list = new List<int>(8);
      Deque<int> deque = new Deque<int>();
      LinkedList<int> linkedlist1 = new LinkedList<int>();
      int dummy;


      for (int i = 0; i < 100000; i++)
      {
        stack.Push(rand.Next());
      }
      for (int i = 0; i < 100000; i++)
      {
        dummy = stack.Pop();
      }
      stack = new Stack<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "Push to Stack...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        stack.Push(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "Pop from Stack...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = stack.Count;
        var isEmpty = stack.Count <= 0;
        dummy = stack.Pop();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      for (int i = 0; i < 100000; i++)
      {
        stackX.Push(rand.Next());
      }
      for (int i = 0; i < 100000; i++)
      {
        dummy = stackX.Pop();
      }
      stackX = new StackX<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "Push to StackX...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        stackX.Push(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "Pop from StackX...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = akkaStack.Count;
        var isEmpty = stackX.IsEmpty;
        dummy = stackX.Pop();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      for (int i = 0; i < 100000; i++)
      {
        queue.Enqueue(rand.Next());
      }
      for (int i = 0; i < 100000; i++)
      {
        dummy = queue.Dequeue();
      }
      queue = new Queue<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "Enqueue to Queue...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        queue.Enqueue(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "Dequeue from Queue...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = queue.Count;
        var isEmpty = queue.Count <= 0;
        dummy = queue.Dequeue();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      for (int i = 0; i < 100000; i++)
      {
        queueX.Enqueue(rand.Next());
      }
      for (int i = 0; i < 100000; i++)
      {
        dummy = queueX.Dequeue();
      }
      queueX = new QueueX<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "Enqueue to QueueX...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        queueX.Enqueue(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "Dequeue from QueueX...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        var isEmpty = queueX.IsEmpty;
        dummy = queueX.Dequeue();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      for (int i = 0; i < 100000; i++)
      {
        deque.AddToFront(rand.Next());
        deque.AddToBack(rand.Next());
      }
      for (int i = 0; i < 100000; i++)
      {
        dummy = deque.RemoveFromFront();
        dummy = deque.RemoveFromBack();
      }

      deque = new Deque<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "AddToBack to Deque...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        deque.AddToBack(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "RemoveFromFront from Deque...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = deque.Count;
        var isEmpty = deque.IsEmpty;
        dummy = deque.RemoveFromFront();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);

      
      deque = new Deque<int>(8);
      sw.Reset();
      Console.Write("{0,40}", "AddToFront to Deque...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        deque.AddToFront(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "RemoveFromBack from Deque...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = deque.Count;
        var isEmpty = deque.IsEmpty;
        dummy = deque.RemoveFromBack();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      sw.Reset();
      Console.Write("{0,40}", "Add to List...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        list.Add(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();

      Console.Write("{0,40}", "AddLast to LinkedList...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        linkedlist1.AddLast(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "RemoveFirst from LinkedList...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = linkedlist1.Count;
        var isEmpty = !linkedlist1.Any();
        dummy = linkedlist1.First.Value;
        linkedlist1.RemoveFirst();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      linkedlist1 = new LinkedList<int>();
      sw.Reset();
      Console.Write("{0,40}", "AddFirst to LinkedList...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        linkedlist1.AddFirst(rand.Next());
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks", sw.ElapsedTicks);
      sw.Reset();
      Console.Write("{0,40}", "RemoveFirst from LinkedList...");
      sw.Start();
      for (int i = 0; i < 100000; i++)
      {
        //var count = linkedlist1.Count;
        var isEmpty = !linkedlist1.Any();
        dummy = linkedlist1.First.Value;
        linkedlist1.RemoveFirst();
        dummy++;
      }
      sw.Stop();
      Console.WriteLine("  Time used: {0,9} ticks\n", sw.ElapsedTicks);


      Console.WriteLine("按任意键退出");
      Console.ReadKey();
    }
  }
}
