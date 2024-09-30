using UnityEngine;
using UnityEditor;

namespace com.aoyon.triangleselector
{
    public class MouseEvents
    {
        private const double RAYCAST_INTEREVAL = 0.01;
        private const float DRAG_THREDHOLD = 10f;
        private readonly Color _selectionColor = new Color(0.6f, 0.7f, 0.8f, 0.25f);

        private PreviewController _previewController;
        
        private bool _isdragging = false;
        private Vector2 _startPoint = new Vector2();
        private Rect _selectionRect = new Rect();
        private double _lastUpdateTime = 0;

        public MouseEvents(PreviewController previewController)
        {
            _previewController = previewController;
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            HandleMouseEvents(sceneView);
            DrawSelectionRectangle();
        }

        private static bool FilterEvent(Event e)
        {
            switch (e.type)
            {
                case EventType.Layout:
                case EventType.Repaint:
                case EventType.ExecuteCommand:
                    return false;
                case EventType.MouseMove:
                case EventType.MouseUp:
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    return true;
                default:
                    return false;
            }
        }

        private void HandleMouseEvents(SceneView sceneView)
        {   
            Event e = Event.current;
            HandleUndoRedoEvent(e);
            if (FilterEvent(e))
            {
                Vector2 endPoint;
                Vector2 mousePos = e.mousePosition;

                //consoleがrectに入っているので多分あまり正確ではない
                float xoffset = 10f;
                float yoffset = 30f; 
                Rect sceneViewRect = new Rect(0, 0, sceneView.position.width -xoffset, sceneView.position.height - yoffset);

                //sceneviewの外側にある場合の初期化処理
                if (!sceneViewRect.Contains(mousePos))
                {
                    HighlightEdgesManager.ClearHighlights();
                    if (_isdragging)
                    {
                        _isdragging = false;
                        _selectionRect = new Rect();
                        //HandleUtility.Repaint();
                        DrawSelectionRectangle();
                    }
                    return;
                }

                //左クリック
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _startPoint = mousePos;
                }
                //左クリック解放
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    //クリック
                    if (!_isdragging)
                    {
                        _previewController.HandleClick(true);
                    }
                    //ドラッグ解放
                    else
                    {
                        endPoint = mousePos;
                        _previewController.HandleDrag(true, _startPoint, endPoint);
                    }
                    
                    _isdragging = false;
                    _selectionRect = new Rect();
                    DrawSelectionRectangle();

                }
                //ドラッグ中
                else if (e.type == EventType.MouseDrag && e.button == 0 && Vector2.Distance(_startPoint, mousePos) >= DRAG_THREDHOLD)
                {
                    _isdragging = true;
                    _selectionRect = new Rect(_startPoint.x, _startPoint.y, mousePos.x - _startPoint.x, mousePos.y - _startPoint.y);
                    double currentTime = EditorApplication.timeSinceStartup;
                    if (currentTime - _lastUpdateTime >= RAYCAST_INTEREVAL)
                    {
                        _lastUpdateTime = currentTime;
                        endPoint = mousePos;
                        _previewController.HandleDrag(false, _startPoint, endPoint);
                    }

                }
                //ドラッグしていないとき
                else if (e.type != EventType.MouseDrag && !_isdragging)
                {
                    double currentTime = EditorApplication.timeSinceStartup;
                    if (currentTime - _lastUpdateTime >= RAYCAST_INTEREVAL)
                    {
                        _lastUpdateTime = currentTime;
                        _previewController.HandleClick(false);
                    }
                }
            
            }

        }

        void HandleUndoRedoEvent(Event e)
        {
            if (e.type == EventType.KeyDown && (e.control || e.command))
            {
                if (e.keyCode == KeyCode.Z) // Ctrl/Cmd + Z
                {
                    _previewController.PerformUndo();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Y) // Ctrl/Cmd + Y
                {
                    _previewController.PerformRedo();
                    e.Use();
                }
            }
        }

        private void DrawSelectionRectangle()
        {
            Handles.BeginGUI();
            GUI.color = _selectionColor;
            GUI.DrawTexture(_selectionRect, EditorGUIUtility.whiteTexture);
            Handles.EndGUI();
        }

    }
}