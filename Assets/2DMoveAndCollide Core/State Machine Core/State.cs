﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public virtual void Tick(){}
    public virtual void OnStateEnter(){}
    public virtual void OnStateExit(){}
}
