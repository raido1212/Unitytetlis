using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{

    public Mino.MinoType type { get; set; } = default;
    public Renderer obj { get; set; } = new Renderer();

}
