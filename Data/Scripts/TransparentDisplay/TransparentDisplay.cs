using System;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using VRage.ModAPI;
using Digi.Utils;

namespace Digi.TransparentDisplay
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class TransparentDisplayMod : MySessionComponentBase
    {
        public static bool init { get; private set; }
        
        public void Init()
        {
            Log.Init();
            Log.Info("Initialized.");
            init = true;
        }
        
        protected override void UnloadData()
        {
            try
            {
                if(init)
                {
                    init = false;
                    Log.Info("Mod unloaded.");
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
            
            Log.Close();
        }
        
        public override void UpdateAfterSimulation()
        {
            if(!init)
            {
                if(MyAPIGateway.Session == null)
                    return;
                
                Init();
            }
        }
    }
    
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), "LargeTransparentDisplay", "MediumTransparentDisplay")]
    public class TransparentDisplay : MyGameLogicComponent
    {
        private bool prevFuntional = true;
        private float transparency = DISPLAY_TRANSPARENCY;
        
        private const float DISPLAY_TRANSPARENCY = 0.5f;
        
        private static readonly Color FRAMELIGHT_COLOR_ON = Color.White;
        private static readonly Color FRAMELIGHT_COLOR_OFF = Color.Black;
        private const float FRAMELIGHT_EMISSIVE_ON = 0.1f;
        private const float FRAMELIGHT_EMISSIVE_OFF = 0;
        
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }
        
        public override void UpdateAfterSimulation()
        {
            try
            {
                if(Entity.Render == null || !Entity.InScene || Entity.MarkedForClose)
                    return;
                
                var panel = Entity as IMyTextPanel;
                
                if(prevFuntional != panel.IsFunctional) // when functional state changes
                {
                    var panelSlim = panel.CubeGrid.GetCubeBlock(panel.Position);
                    var panelDef = ((MyCubeBlock)panel).BlockDefinition;
                    
                    prevFuntional = panel.IsFunctional;
                    transparency = (panelSlim.BuildLevelRatio >= panelDef.CriticalIntegrityRatio ? DISPLAY_TRANSPARENCY : 0); // make it opaque if build model is used
                }
                
                if(Math.Abs(Entity.Render.Transparency - transparency) > 0.001f)
                {
                    Entity.Render.Transparency = transparency;
                    Entity.Render.RemoveRenderObjects();
                    Entity.Render.AddRenderObjects();
                    
                    if(panel.IsFunctional)
                        Entity.SetEmissiveParts("ScreenArea", Color.White, 1); // fixes the screen being non-emissive
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }
        
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return Entity.GetObjectBuilder(copy);
        }
    }
}