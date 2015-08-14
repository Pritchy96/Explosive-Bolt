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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Door))]
    class ExplosiveBolt : MyGameLogicComponent
    {
        MyObjectBuilder_EntityBase m_objectBuilder;
        private string SubTypeNameLarge = "LargeExplosiveBolt", SubTypeNameSmall = "SmallExplosiveBolt";
        private bool _didInit = false;
        IMyDoor Door;

        public override void Close()
        {
            //MyLogger.Default.WriteLine("Close()");
            Door.DoorStateChanged -= Detonate;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //Get door that script it attatched to.
            Door = Entity as IMyDoor;
            //Set up detonation callback when 'door' is opened.
            Door.DoorStateChanged += Detonate;
            
            m_objectBuilder = objectBuilder;
           
            //DEBUG
            MyLogger.Default.ToScreen = false ;
            //MyLogger.Default.WriteLine("Successfully placed a bolt");
        }

        void Detonate(bool isOpening)
        {
            //MyLogger.Default.WriteLine(isOpening.ToString());

            //if door is opening and is actually an explosive bolt (not a door).
            if (isOpening && Door.BlockDefinition.SubtypeName.Contains("ExplosiveBolt"))
            {
                if (Door != null)
                {
                    //MyLogger.Default.WriteLine("Bolt firing");
                    ((Door.CubeGrid.GetCubeBlock(Door.Position)) as IMyDestroyableObject).DoDamage(2000, MyDamageType.Weld, false);

                    //Apply Damage
                    //slimBlock.ApplyAccumulatedDamage(false);
                }
                else
                {
                    //MyLogger.Default.WriteLine("Door is not initialised");
                }
            }
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
            MyLogger.DefaultClose();
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


