using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Digi.TransparentDisplay
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class TransparentDisplayMod : MySessionComponentBase
    {
        private bool init = false;

        public override void LoadData()
        {
            Log.SetUp("Transparent Display", 725801285, "TransparentDisplay");
        }

        public void Init()
        {
            init = true;
            Log.Init();

            MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
        }

        protected override void UnloadData()
        {
            init = false;
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

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "LargeTransparentDisplay", "MediumTransparentDisplay", "SmallTransparentDisplay")]
    public class TransparentDisplay : MyGameLogicComponent
    {
        private IMyTextPanel panel;
        private bool prevFunctional = true;
        private float transparency = DISPLAY_TRANSPARENCY;

        private const float DISPLAY_TRANSPARENCY = 0.5f;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            panel = Entity as IMyTextPanel;
            panel.IsWorkingChanged += IsWorkingChanged;

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void Close()
        {
            panel.IsWorkingChanged -= IsWorkingChanged;
        }

        private void IsWorkingChanged(IMyCubeBlock block)
        {
            if(block.IsWorking)
                block.SetEmissivePartsForSubparts("Functional", Color.LightSteelBlue, 1f);
            else
                block.SetEmissivePartsForSubparts("Functional", Color.LightGoldenrodYellow, 0f);
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if(panel == null || panel.Render == null || !panel.InScene || panel.MarkedForClose)
                    return;

                if(prevFunctional != panel.IsFunctional) // when functional state changes
                {
                    var panelSlim = panel.CubeGrid.GetCubeBlock(panel.Position);
                    var panelDef = ((MyCubeBlock)panel).BlockDefinition;

                    prevFunctional = panel.IsFunctional;
                    transparency = (panelSlim.BuildLevelRatio >= panelDef.CriticalIntegrityRatio ? DISPLAY_TRANSPARENCY : 0); // make it opaque if build model is used
                }

                if(Math.Abs(panel.SlimBlock.Dithering - transparency) > 0.001f)
                {
                    panel.SlimBlock.Dithering = transparency;

                    if(panel.IsFunctional)
                        panel.SetEmissiveParts("ScreenArea", Color.White, 1); // fixes the screen being non-emissive
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }
    }
}