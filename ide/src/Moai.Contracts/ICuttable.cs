﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moai
{
    public interface ICuttable
    {
        bool CanCut { get; }

        void Cut();
    }
}
