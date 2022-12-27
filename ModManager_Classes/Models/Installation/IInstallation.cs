﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imya.Models.Installation
{
    public interface IInstallation : IProgress<float>
    {
        float Progress { get; }
        bool IsInstalling { get; }
        bool IsAbortable { get; }
        String ID { get; }

        IText? HeaderText { get; }
        IText? AdditionalText { get; }
        bool HasAdditionalText { get; }
        IInstallationStatus? Status { get; set; }
        void SetProgressRange(float Min, float Max);
    }

    public interface IInstallationStatus
    {
        public IText Localized { get; }
    }
}
