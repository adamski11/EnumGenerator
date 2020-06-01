using Adamski11.EnumGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumContainerExample : EnumContainer
{
    public EnumInfo[] exampleEnums;

    public override EnumInfo[] GetEnums()
    {
        return exampleEnums;
    }
}
