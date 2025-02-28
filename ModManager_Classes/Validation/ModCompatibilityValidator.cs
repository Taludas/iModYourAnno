﻿using Imya.Models;
using Imya.Models.Attributes;
using Imya.Models.ModMetadata;
using System.Collections.Immutable;

namespace Imya.Validation
{
    public class ModCompatibilityValidator : IModValidator
    {
        public void Validate(IEnumerable<Mod> changed, IReadOnlyCollection<Mod> all)
        {
            foreach (var mod in all)
                ValidateSingle(mod, all);
        }

        private static void ValidateSingle(Mod mod, IReadOnlyCollection<Mod> collection)
        {
            mod.Attributes.RemoveAttributesByType(AttributeType.UnresolvedDependencyIssue);
            mod.Attributes.RemoveAttributesByType(AttributeType.ModCompabilityIssue);
            mod.Attributes.RemoveAttributesByType(AttributeType.ModReplacedByIssue);

            // skip dependency check if inactive or standalone
            if (!mod.IsActiveAndValid || collection is null)
                return;

            var unresolvedDeps = GetUnresolvedDependencies(mod.Modinfo, collection);
            if (unresolvedDeps.Any())
                mod.Attributes.AddAttribute(new ModDependencyIssueAttribute(unresolvedDeps));

            var incompatibles = GetIncompatibleMods(mod.Modinfo, collection);
            if (incompatibles.Any())
                mod.Attributes.AddAttribute(ModCompabilityAttributeFactory.Get(incompatibles));

            Mod? newReplacementMod = HasBeenDeprecated(mod.Modinfo, collection) ?? IsNewestOfID(mod, collection);
            if (newReplacementMod is not null && newReplacementMod != mod)
                mod.Attributes.AddAttribute(new ModReplacedByIssue(newReplacementMod));
        }

        private static IEnumerable<string> GetUnresolvedDependencies(Modinfo modinfo, IReadOnlyCollection<Mod> collection)
        {
            if (modinfo.ModDependencies is null)
                yield break;

            foreach (var dep in modinfo.ModDependencies)
            {
                if (!collection.Any(x => x.Modinfo.ModID is not null 
                    && (x.Modinfo.ModID.Equals(dep) || x.SubMods?.Find(submod => submod.ModID.Equals(dep)) is not null)
                    && x.IsActiveAndValid))
                    yield return dep;
            }
        }

        private static IEnumerable<Mod> GetIncompatibleMods(Modinfo modinfo, IReadOnlyCollection<Mod> collection)
        {
            if (collection is null || modinfo.IncompatibleIds is null || modinfo.ModID is null) 
                yield break;
            
            foreach (var inc in modinfo.IncompatibleIds)
            {
                var incompatibles = collection.Where(x => x.Modinfo.ModID is not null && x.Modinfo.ModID.Equals(inc) && x.IsActiveAndValid);
                foreach (var result in incompatibles)
                    yield return result;
            }
        }

        private static Mod? HasBeenDeprecated(Modinfo modinfo, IReadOnlyCollection<Mod> collection)
        {
            if (collection is null || modinfo.ModID is null)
                return null;

            return collection.FirstOrDefault(x => x.Modinfo?.DeprecateIds?.Contains(modinfo.ModID) ?? false);
        }

        private static Mod? IsNewestOfID(Mod mod, IReadOnlyCollection<Mod> collection)
        {
            if (collection is null || mod.Modinfo.ModID is null)
                return null;

            return collection.Where(x => x.Modinfo.ModID == mod.Modinfo.ModID).OrderBy(x => x.Version).LastOrDefault();
        }
    }
}
