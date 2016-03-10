using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storm;
using Storm.Collections;
using Storm.ExternalEvent;
using Storm.StardewValley.Event;
using Storm.StardewValley.Proxy;
using Storm.StardewValley.Wrapper;

namespace QualityExtender
{
    [Mod]
    public class QualityExtender : DiskResource
    {

        public void InitializeCallback(InitializeEvent @event)
        {
            
        }

        [Subscribe]
        public void ChatCallback(ChatMessageEnteredEvent @event)
        {
            Command c = Command.ParseCommand(@event.ChatText);
            if (c.Name == "newitem")
            {
                Console.WriteLine("GIVING ITEM");
                Item i = @event.ProxyObject(new CObject(290, 1));
                if (@event.Root.Player.Items[8] != null)
                {
                    @event.Root.Player.Items[8].Stack += 1;
                }
                else
                    @event.Root.Player.Items[8] = i;
            }
        }

        public class CObject : ObjectDelegate
        {
            public int spi;
            public int stack;

            public CObject(int parentSpriteSheetIndex, int initialStack)
            {
                this.spi = parentSpriteSheetIndex;
                this.stack = initialStack;
            }

            public override object[] GetConstructorParams()
            {
                return new object[] {spi, stack};
            }
        }
    }

    public class QualityConfig : Config
    {
        public int MaxQualityLevel { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            MaxQualityLevel = 10;
            return this;
        }
    }
}
