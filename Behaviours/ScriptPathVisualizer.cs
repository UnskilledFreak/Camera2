using System.Collections.Generic;
using Camera2.MovementScript;
using UnityEngine;
using UnityEngine.Rendering;

namespace Camera2.Behaviours;

internal class ScriptPathVisualizer : MonoBehaviour
{
    private const float PathWidth = 0.02f;
    private const float ArrowLength = 0.4f;
    private const float ArrowShaftWidth = 0.02f;
    private const float ArrowHeadWidth = 0.09f;
    private const float ArrowHeadStart = 0.65f;
    private const float ArrowHeadFull = 0.7f;
    private const float SelectedArrowScale = 1.6f;

    private static readonly Color PathStartColor = new Color(0.2f, 1f, 0.3f, 0f);
    private static readonly Color PathEndColor = new Color(1f, 0.3f, 0.2f, 0f);
    private static readonly Color LoopColor = new Color(0.45f, 0.45f, 0.45f, 0f);
    private static readonly Color ArrowColor = new Color(1f, 1f, 1f, 0f);
    private static readonly Color SelectedArrowColor = new Color(1f, 0.9f, 0.1f, 0f);

    private static readonly AnimationCurve ArrowWidth = new AnimationCurve(
        new Keyframe(0f, ArrowShaftWidth),
        new Keyframe(ArrowHeadStart, ArrowShaftWidth),
        new Keyframe(ArrowHeadFull, ArrowHeadWidth),
        new Keyframe(1f, 0f)
    );

    private static Material material;

    private readonly List<LineRenderer> _arrows = new List<LineRenderer>();
    private readonly List<LineRenderer> _lines = new List<LineRenderer>();
    private LineRenderer _path;
    private LineRenderer _loopSegment;
    private Camera _hiddenFrom;

    internal static ScriptPathVisualizer Create(Cam2 editorCam)
    {
        var visualizer = new GameObject("Cam2_ScriptPath").AddComponent<ScriptPathVisualizer>();
        visualizer.transform.SetParent(editorCam.transform, false);
        visualizer._hiddenFrom = editorCam.Camera;

        return visualizer;
    }

    internal void Show(Script script, int selectedIndex)
    {
        var count = script?.Frames.Count ?? 0;

        _path ??= CreateLine("Path", PathWidth);
        _path.gameObject.SetActive(count >= 2);
        if (count >= 2)
        {
            _path.positionCount = count;
            for (var i = 0; i < count; i++)
            {
                _path.SetPosition(i, script.Frames[i].Position);
            }

            _path.startColor = PathStartColor;
            _path.endColor = PathEndColor;
        }

        var showLoop = count >= 2 && script.Loop && script.Frames[0].Duration > 0f;
        _loopSegment ??= CreateLine("Loop", PathWidth * 0.6f);
        _loopSegment.gameObject.SetActive(showLoop);
        if (showLoop)
        {
            _loopSegment.positionCount = 2;
            _loopSegment.SetPosition(0, script.Frames[count - 1].Position);
            _loopSegment.SetPosition(1, script.Frames[0].Position);
            _loopSegment.startColor = _loopSegment.endColor = LoopColor;
        }

        while (_arrows.Count < count)
        {
            var arrow = CreateLine("Arrow", 1f);
            arrow.widthCurve = ArrowWidth;
            arrow.positionCount = 4;
            _arrows.Add(arrow);
        }

        for (var i = 0; i < _arrows.Count; i++)
        {
            var arrow = _arrows[i];
            arrow.gameObject.SetActive(i < count);
            if (i >= count)
            {
                continue;
            }

            var frame = script.Frames[i];
            var isSelected = i == selectedIndex;
            var scale = isSelected ? SelectedArrowScale : 1f;
            var direction = frame.Rotation * Vector3.forward * (ArrowLength * scale);

            arrow.SetPosition(0, frame.Position);
            arrow.SetPosition(1, frame.Position + (direction * ArrowHeadStart));
            arrow.SetPosition(2, frame.Position + (direction * ArrowHeadFull));
            arrow.SetPosition(3, frame.Position + direction);
            arrow.widthMultiplier = scale;
            arrow.startColor = arrow.endColor = isSelected ? SelectedArrowColor : ArrowColor;
        }
    }

    private LineRenderer CreateLine(string lineName, float width)
    {
        var line = new GameObject(lineName).AddComponent<LineRenderer>();
        line.transform.SetParent(transform, false);
        line.sharedMaterial = GetMaterial();
        line.useWorldSpace = false;
        line.widthMultiplier = width;
        line.numCornerVertices = 4;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        _lines.Add(line);

        return line;
    }

    private static Material GetMaterial()
    {
        if (material != null)
        {
            return material;
        }

        var shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
        {
            return material = new Material(Plugin.ShaderVolumetricBlit);
        }

        material = new Material(shader);
        material.SetInt("_SrcBlend", (int)BlendMode.One);
        material.SetInt("_DstBlend", (int)BlendMode.Zero);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZWrite", 1);

        return material;
    }

    private void OnEnable()
    {
        Camera.onPreCull += HandlePreCull;
        Camera.onPostRender += HandlePostRender;
    }

    private void OnDisable()
    {
        Camera.onPreCull -= HandlePreCull;
        Camera.onPostRender -= HandlePostRender;
    }

    private void HandlePreCull(Camera renderingCamera)
    {
        if (renderingCamera == _hiddenFrom)
        {
            SetLinesEnabled(false);
        }
    }

    private void HandlePostRender(Camera renderingCamera)
    {
        if (renderingCamera == _hiddenFrom)
        {
            SetLinesEnabled(true);
        }
    }

    private void SetLinesEnabled(bool isEnabled)
    {
        foreach (var line in _lines)
        {
            line.enabled = isEnabled;
        }
    }
}