﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moai
{
    public interface ISavable
    {
        bool CanSave { get; }
        string SaveName { get; }

        void SaveFile();
        void SaveFileAs();
    }
}
