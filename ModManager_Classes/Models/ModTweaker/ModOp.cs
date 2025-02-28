﻿using Imya.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Imya.Models.ModTweaker
{
    public class ModOp
    {
        public String? ID;
        public IEnumerable<XmlNode> Code;
        public string? Skip;

        //Mod Op related things
        public String Type;
        public String? GUID;
        public String? Path;

        public bool IsValid => GUID is String || Path is String;
        public bool HasID => ID is String;

        public static ModOp? FromXmlNode(XmlNode ModOp)
        {
            string? type = null;
            if (ModOp.Name.ToLower() == "include")
                type = "include";
            if (type is null && ModOp.TryGetAttribute(TweakerConstants.TYPE, out string? ModOpType))
                type = ModOpType;

            if (type is not null)
            {
                ModOp.TryGetAttribute(TweakerConstants.GUID, out String? Guid);
                ModOp.TryGetAttribute(TweakerConstants.PATH, out String? Path);
                ModOp.TryGetAttribute(TweakerConstants.MODOP_ID, out String? ID);
                return new ModOp
                {
                    ID = ID!,
                    Code = ModOp.ChildNodes.Cast<XmlNode>().ToList(),
                    Type = type,
                    GUID = Guid,
                    Path = Path,
                    Skip = ModOp.Attributes?["Skip"]?.Value
                };
            }
            return null;
        }

        public ModOp Clone()
        {
            return new ModOp()
            {
                ID = this.ID,
                Code = this.Code.Select(x => x.CloneNode(true)).ToList(),
                Type = this.Type,
                GUID = this.GUID,
                Path = this.Path
            };        
        }
    }

}
