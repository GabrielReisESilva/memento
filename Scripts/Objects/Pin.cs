using System.Collections;
using System.Collections.Generic;
using Geogram;
using Mapbox.Utils;
using UnityEngine;

public class Pin : MonoBehaviour
{
    public PinType type;
    public Vector2d coordinates;
    public Memento memento;
    public SpriteRenderer renderer;
    public Sprite redMemento;
    public Sprite blueMemento;
    public Sprite grayMemento;

    private Sprite originalColor;

    public void SetData(PinType type, Vector2d coordinates, Memento memento)
    {
        this.type = type;
        this.coordinates = coordinates;
        this.memento = memento;
    }

    public void CheckOwner(string userName)
    {
        return;
        if (memento.user_id == userName)
        {
            renderer.sprite = blueMemento;
        }
        else
        {
            renderer.sprite = redMemento;
        }
        originalColor = renderer.sprite;
    }

    public void IsFar()
    {
        renderer.sprite = grayMemento;
    }

    public void IsClose()
    {
        renderer.sprite = originalColor;
    }
}
