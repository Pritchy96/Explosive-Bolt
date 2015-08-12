using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Common.ObjectBuilders.Voxels;
using Sandbox.Common.ObjectBuilders.VRageData;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRage.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExplosiveBolt
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Warhead))]
    //[MyEntitySubIdComponentDescriptor("LargeBlockRadar")]
    //[MyEntitySubIdComponentDescriptor("SmallBlockRadar")]
    public class RadarLogic : MyGameLogicComponent
    {
        // Contains the list of radar blocks in the world
        public static Dictionary<IMyEntity, MyTuple<IMyEntity, DateTime>> LastRadarUpdate
        {
            get
            {
                return m_lastUpdate;
            }
        }

        private MyObjectBuilder_EntityBase m_objectBuilder = null;
        private static Dictionary<IMyEntity, MyTuple<IMyEntity, DateTime>> m_lastUpdate = null;

        /// <summary>
        /// So, uhm.  Init on a beacon passes a null objectBuilder?  This can't be right.
        /// </summary>
        /// <param name="objectBuilder"></param>
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (m_lastUpdate == null)
                m_lastUpdate = new Dictionary<IMyEntity, MyTuple<IMyEntity, DateTime>>();

            Sandbox.ModAPI.Ingame.IMyWarhead warhead = (Sandbox.ModAPI.Ingame.IMyWarhead)Entity;

            //if the object attached subtypeName is explosiveBolt, we're attatched to the right object.
            //This means it will not affect normal warheads.
            if (warhead.BlockDefinition.SubtypeName.Contains("ExplosiveBolt"))
            {
                //?
                if (!m_lastUpdate.ContainsKey(Entity))
                {
                    m_lastUpdate.Add(Entity, new MyTuple<IMyEntity, DateTime>(Entity, DateTime.Now));
                }
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return m_objectBuilder;
        }

        public override void Close()
        {
            if (Entity == null)
                return;

            if (m_lastUpdate.ContainsKey(Entity))
                m_lastUpdate.Remove(Entity);
        }
    }
}
