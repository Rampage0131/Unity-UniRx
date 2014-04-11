﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnityRx.Tests
{
    [TestClass]
    public class SchedulerTest
    {
        private static string[] ScheduleTasks(IScheduler scheduler)
        {
            var list = new List<string>();

            Action leafAction = () => list.Add("----leafAction.");
            Action innerAction = () =>
            {
                list.Add("--innerAction start.");
                scheduler.Schedule(leafAction);
                list.Add("--innerAction end.");
            };
            Action outerAction = () =>
            {
                list.Add("outer start.");
                scheduler.Schedule(innerAction);
                list.Add("outer end.");
            };
            scheduler.Schedule(outerAction);

            return list.ToArray();
        }

        [TestMethod]
        public void CurrentThread()
        {
            var hoge = ScheduleTasks(Scheduler.CurrentThread);
            hoge.Is("outer start.", "outer end.", "--innerAction start.", "--innerAction end.", "----leafAction.");
        }
        [TestMethod]
        public void CurrentThread2()
        {
            var scheduler = Scheduler.CurrentThread;

            var list = new List<string>();
            scheduler.Schedule(() =>
            {
                list.Add("one");

                scheduler.Schedule(TimeSpan.FromSeconds(3), () =>
                {
                    list.Add("after 3");
                });

                scheduler.Schedule(TimeSpan.FromSeconds(1), () =>
                {
                    list.Add("after 1");
                });
            });

            list.Is("one", "after 1", "after 3");
        }

        [TestMethod]
        public void Immediate()
        {
            var hoge = ScheduleTasks(Scheduler.Immediate);
            hoge.Is("outer start.", "--innerAction start.", "----leafAction.", "--innerAction end.", "outer end.");
        }
    }
}
