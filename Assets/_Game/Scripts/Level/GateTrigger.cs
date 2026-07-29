using System;
using TMPro;
using UnityEngine;

// A single gate the crowd runs through. The operator and value are
// hardcoded per-prefab-instance in the Inspector (no randomisation).
public class GateTrigger : MonoBehaviour
{
    public enum Operator { Add, Subtract, Multiply, Divide }

    [Header("Gate Settings (set per gate in Inspector)")]
    [SerializeField] GateTriggerParent  _parent;
    [SerializeField] TextMeshPro  textLabel;
    // Which maths operation this gate applies.
    [SerializeField] private Operator op = Operator.Add;
    // The value used by the operation (e.g. 25 for +25, 3 for x3).
    [SerializeField] private float value = 25f;

    // Ensures the gate only fires once per crowd pass.
    private bool consumed;

    // Fires when the crowd root (tagged "Player") enters the gate.

    private void Start()
    {
        SetLabel();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore anything that is not the crowd root, and fire only once.
        if (consumed) return;
        if (_parent.HasTriggered) return;
        if (!other.CompareTag("Player")) return;

        consumed = true;
        _parent.HasTriggered = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(AudioManager.SfxType.GateTrigger);
        }
        ApplyOperation();
    }
    
    private void SetLabel()
    {
        // Route to the correct crowd operation.
        switch (op)
        {
            case Operator.Add:
                textLabel.text = $"+{value}";
                break;
            case Operator.Subtract:
                textLabel.text = $"-{value}";
                break;
            case Operator.Multiply:
                textLabel.text = $"*{value}";
                break;
            case Operator.Divide:
                textLabel.text = $"÷{value}";
                break;
        }
    }

    // Calls the matching CrowdManager method based on the operator.
    private void ApplyOperation()
    {
        // Route to the correct crowd operation.
        switch (op)
        {
            case Operator.Add:
                CrowdManager.Instance.AddCharacters(Mathf.RoundToInt(value));
                break;
            case Operator.Subtract:
                CrowdManager.Instance.RemoveCharacters(Mathf.RoundToInt(value));
                break;
            case Operator.Multiply:
                CrowdManager.Instance.MultiplyCrowd(value);
                break;
            case Operator.Divide:
                CrowdManager.Instance.DivideCrowd(value);
                break;
        }
    }
}
