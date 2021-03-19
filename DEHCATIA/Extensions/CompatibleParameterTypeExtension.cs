// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompatibleParameterTypeExtension.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
// 
//    This file is part of DEHPEcosimPro
// 
//    The DEHPEcosimPro is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
// 
//    The DEHPEcosimPro is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Lesser General Public License
//    along with this program; if not, write to the Free Software Foundation,
//    Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHCATIA.Extensions
{
    using CDP4Common.SiteDirectoryData;

    using DevExpress.CodeParser;

    /// <summary>
    /// Performs validation upon a <see cref="ParameterType"/>
    /// </summary>
    public static class CompatibleParameterTypeExtension
    {
        public static bool IsCompoundParameterTypeCompatibleWithShape(this ParameterType parameterType)
        {
            if (parameterType is CompoundParameterType compoundParameterType)
            {
                var hasTheRightComponents = compoundParameterType.HasTheRightComponentsForShape();
            }

            return true;
        }

        public static bool HasTheRightComponentsForShape(this CompoundParameterType compoundParameterType)
        {
            if (compoundParameterType.Component.Count == 11)
            {
                return compoundParameterType.Component[0].ShortName == "kind" 
                       && compoundParameterType.Component[1].ShortName == "len" 
                       && compoundParameterType.Component[2].ShortName == "wid_diam";
            }

            return false;
        }
    }
}
