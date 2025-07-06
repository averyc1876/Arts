using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using XSkills;
using ArtsXSlills;
using System;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.Server;

namespace ArtsXSkills
{
    public class ArtsXSkillsModSystem : ModSystem
    {        
        public ICoreAPI Api { get; private set; }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ArtsXSkillsItemPlantableSeed", typeof(ArtsXSkillsItemPlantableSeed));
        }
        
    }
}
