﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NE4S.Notes.Interface
{
    public interface ILongNote
    {
        bool Put(in IStepNote step);
        bool UnPut(IStepNote step);
    }
}
