﻿using System.Collections.Generic;
using SAUtils.Interface;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace SAUtils.DataStructures
{

    public sealed class InterimSaPosition
    {
        private readonly List<IInterimSaItem> _intermediateSaItems;
        public int Position { get; private set; }
        private string GlobalMajorAllele { get; set; }
        private bool IsReferenceMinor { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public InterimSaPosition()
        {
            _intermediateSaItems = new List<IInterimSaItem>();
        }

        public void AddSaItems(List<IInterimSaItem> saItems)
        {
            Position = saItems[0].Position;

            _intermediateSaItems.AddRange(saItems);
        }

        public ISaPosition Convert()
        {
            var dataSources = new List<ISaDataSource>();
            foreach (var intermediateSaItem in _intermediateSaItems)
            {
                if (intermediateSaItem.KeyName == InterimSaCommon.RefMinorTag)
                {
                    IsReferenceMinor = true;
                    if (intermediateSaItem is SaMiscellanies miscItem) GlobalMajorAllele = miscItem.GlobalMajorAllele;
                }
                else
                {
                    dataSources.Add(intermediateSaItem as ISaDataSource);
                }
            }

            return new SaPosition(dataSources.ToArray(), GlobalMajorAllele);
        }
    }
}
