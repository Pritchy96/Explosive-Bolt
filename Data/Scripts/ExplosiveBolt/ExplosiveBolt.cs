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

using IMyCubeBlock = Sandbox.ModAPI.IMyCubeBlock;
namespace ExplosiveBolt
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Warhead))]
    class ExplosiveBolt : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase m_objectBuilder;
        private string SubTypeNameLarge = "LargeExplosiveBolt", SubTypeNameSmall = "SmallExplosiveBolt";
        private bool _didInit = false;
        IMyCubeBlock block;
        IMySlimBlock slimBlock;

        public override void Close()
        {
            //MyLogger.Default.WriteLine("Close()");
            if (block == null)
            {
                //MyLogger.Default.WriteLine("Block is null in Close()");
                return;
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

            m_objectBuilder = objectBuilder;
            //DEBUG
            //MyLogger.Default.ToScreen = false;
            //MyLogger.Default.WriteLine("Successfully placed a bolt");
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            DoInit();
        }

        public override void UpdateAfterSimulation10()
        {
            var blockDef = (MyObjectBuilder_Warhead)((IMyCubeBlock)Entity).GetObjectBuilderCubeBlock();

            //Only bother if it's a explosive bolt..
            if (block.BlockDefinition.SubtypeName == SubTypeNameLarge || block.BlockDefinition.SubtypeName == SubTypeNameSmall)
            {
                //If countdown timer is <= 0, time to destroy cube.
                if (blockDef.CountdownMs <= 0)
                {
                    //Damage
                    ((IMyDestroyableObject)(slimBlock)).DoDamage(5000, MyDamageType.Weld, false);
                    //Apply Damage
                    slimBlock.ApplyAccumulatedDamage(false);
                }
                base.UpdateAfterSimulation10();
            }
        }

        void DoInit()
        {
            //MyLogger.Default.WriteLine("DoInit()");
            if (_didInit) return;
            _didInit = true;

            if (Entity == null)
            {
                //MyLogger.Default.WriteLine("Block is null");
                _didInit = false;
                return;
            }

            block = (IMyCubeBlock)Entity;
            //Get SlimBlock from IMyCubeBlock
            slimBlock = block.CubeGrid.GetCubeBlock(block.Position);
            //Output SubTypeID
            //MyLogger.Default.WriteLine(block.BlockDefinition.SubtypeName.ToString());
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return m_objectBuilder;
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class LoggerSession : MySessionComponentBase
    {
        protected override void UnloadData()
        {
            base.UnloadData();
            //MyLogger.DefaultClose();
        }
    }

    /// <summary>
    /// Borrowed and modified from official API sample mission mod.  Thanks!
    /// </summary>
    class MyLogger
    {
        private System.IO.TextWriter m_writer;
        private int m_indent;
        private StringBuilder m_cache = new StringBuilder();

        private bool _isFileOpen;
        private bool _isClosed;
        private string _fileName;

        public bool ToScreen { get; set; }

        private static MyLogger _sDefault;

        public static MyLogger Default
        {
            get { return _sDefault ?? (_sDefault = new MyLogger("DefaultLog.txt")); }
        }

        public static void DefaultClose()
        {
            if (_sDefault != null) _sDefault.Close();
        }

        public MyLogger(string logFile)
        {
            _fileName = logFile;
            ReadyFile();
        }

        private bool ReadyFile()
        {
            if (_isFileOpen) return true;
            if (MyAPIGateway.Utilities != null)
            {
                m_writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(_fileName, typeof(MyLogger));
                _isFileOpen = true;
            }
            return _isFileOpen;
        }

        public void IncreaseIndent()
        {
            m_indent++;
        }

        public void DecreaseIndent()
        {
            if (m_indent > 0)
                m_indent--;
        }

        public void WriteLine(string text)
        {
            if (ToScreen && MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.ShowMessage("Log", text);
            }
            if (_isClosed) return;
            if (!ReadyFile())
            {
                m_cache.Append(text);
                return;
            }
            if (m_cache.Length > 0)
                m_writer.WriteLine(m_cache);
            m_cache.Clear();
            m_cache.Append(DateTime.Now.ToString("[HH:mm:ss] "));
            for (int i = 0; i < m_indent; i++)
                m_cache.Append("\t");
            m_writer.WriteLine(m_cache.Append(text));
            m_writer.Flush();
            m_cache.Clear();
        }

        public void Write(string text)
        {
            if (ToScreen)
            {
                MyAPIGateway.Utilities.ShowMessage("Log", text);
            }
            m_cache.Append(text);
        }


        internal void Close()
        {
            _isClosed = true;
            if (m_cache.Length > 0)
                m_writer.WriteLine(m_cache);
            m_writer.Flush();
            m_writer.Close();
        }
    }
}


