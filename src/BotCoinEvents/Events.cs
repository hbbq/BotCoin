using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotCoinEvents
{

    public class Events
    {

        public interface IEvent
        {
            
        }

        public abstract class BaseEvent : IEvent
        {
            public BaseEvent()
            {
            }
        }

        public class TickEvent : BaseEvent
        {

            public decimal mid { get; set; }
            public decimal bid { get; set; }
            public decimal ask { get; set; }
            public decimal last_price { get; set; }
            public decimal low { get; set; }
            public decimal high { get; set; }
            public decimal volume { get; set; }

            public TickEvent() : base()
            {
            }

        }

    }

}
