using System.Collections.Generic;
using System.Linq;

namespace com.aoyon.triangleselector
{
    public class HistoryManager
    {
        private const int MaxStackSize = 20;
        private List<HashSet<int>> undoStack = new List<HashSet<int>>();
        private List<HashSet<int>> redoStack = new List<HashSet<int>>();

        public void AddState(HashSet<int> state)
        {
            if (undoStack.Count >= MaxStackSize)
            {
                undoStack.RemoveAt(0);
            }
            undoStack.Add(new HashSet<int>(state));
            redoStack.Clear();
        }

        public HashSet<int> Undo(HashSet<int> currentState)
        {
            if (undoStack.Count > 0)
            {
                if (redoStack.Count >= MaxStackSize)
                {
                    redoStack.RemoveAt(0);
                }
                redoStack.Add(new HashSet<int>(currentState));
                currentState = undoStack[undoStack.Count - 1];
                undoStack.RemoveAt(undoStack.Count - 1);
            }
            return new HashSet<int>(currentState);
        }

        public HashSet<int> Redo(HashSet<int> currentState)
        {
            if (redoStack.Count > 0)
            {
                if (undoStack.Count >= MaxStackSize)
                {
                    undoStack.RemoveAt(0);
                }
                undoStack.Add(new HashSet<int>(currentState));
                currentState = redoStack[redoStack.Count - 1];
                redoStack.RemoveAt(redoStack.Count - 1);
            }
            return new HashSet<int>(currentState);
        }
    }

    public class TriangleSelectionManager
    {
        private HistoryManager _history;
        private HashSet<int> _allTriangleIndices;
        private HashSet<int> _selectedTriangleIndices;

        public TriangleSelectionManager(HashSet<int> allTriangleIndices, IReadOnlyCollection<int> defaultselection)
        {
            _history = new HistoryManager();
            _allTriangleIndices = new HashSet<int>(allTriangleIndices);
            _selectedTriangleIndices = new HashSet<int>(defaultselection);
        }

        public void SelectAllTriangles()
        {
            _history.AddState(_selectedTriangleIndices);
            _selectedTriangleIndices = new HashSet<int>(_allTriangleIndices);
        }

        public void UnselectAllTriangles()
        {
            _history.AddState(_selectedTriangleIndices);
            _selectedTriangleIndices = new HashSet<int>();
        }

        public void ReverseAllTriangles()
        {
            _history.AddState(_selectedTriangleIndices);
            var reversedSelection = new HashSet<int>(_allTriangleIndices);
            reversedSelection.ExceptWith(_selectedTriangleIndices);
            _selectedTriangleIndices = reversedSelection;
        }

        public void UpdateSelection(HashSet<int> indices, bool isPreviewSelected)
        {
            _history.AddState(_selectedTriangleIndices);
            if (isPreviewSelected)
            {
                _selectedTriangleIndices.ExceptWith(indices);
            }
            else
            {
                _selectedTriangleIndices.UnionWith(indices);
            }
        }

        public void Undo()
        {
            _selectedTriangleIndices = _history.Undo(_selectedTriangleIndices);
        }

        public void Redo()
        {
            _selectedTriangleIndices = _history.Redo(_selectedTriangleIndices);
        }

        public HashSet<int> GetAllTriangles()
        {
            return new HashSet<int>(_allTriangleIndices);
        }

        public HashSet<int> GetSelectedTriangles()
        {
            return new HashSet<int>(_selectedTriangleIndices);
        }

        public HashSet<int> GetUnselectedTriangles()
        {
            var unselectedTriangles = new HashSet<int>(_allTriangleIndices);
            unselectedTriangles.ExceptWith(_selectedTriangleIndices);
            return new HashSet<int>(unselectedTriangles);
        }

        public HashSet<int> GetUniqueTriangles(HashSet<int> triangleIndices, bool isPreviewSelected)
        {
            if (isPreviewSelected)
            {
                return triangleIndices.Intersect(_selectedTriangleIndices).ToHashSet();
            }
            else
            {
                return triangleIndices.Intersect(GetUnselectedTriangles()).ToHashSet();
            }
        }
    }
}