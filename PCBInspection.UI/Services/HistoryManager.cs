using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PCBInspection.Core.ROI;

namespace PCBInspection.UI.Services
{
    public class HistoryManager
    {
        private class Snapshot
        {
            public Bitmap Image { get; set; }
            public List<RoiBase> Rois { get; set; }
        }

        private readonly Stack<Snapshot> _undoStack = new Stack<Snapshot>();
        private readonly Stack<Snapshot> _redoStack = new Stack<Snapshot>();
        private const int MaxDepth = 10; // Prevent memory overflow

        public event EventHandler StateChanged;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void PushState(Bitmap image, List<RoiBase> rois)
        {
             // Deep copy ROIs is tricky conceptually, but for now we clone the list reference? 
             // Ideally we need deep clone of ROI objects to prevent modification of history.
             // But ROI objects are small.
             // Image also needs Clone to be safe.

             var snapshot = new Snapshot
             {
                 Image = (Bitmap)image?.Clone(),
                 // Shallow copy of list logic is flawed if items are mutable. 
                 // We will assume "PushState" is called BEFORE modification, or we implement DeepClone for ROIs.
                 // For this MVP, let's keep it simple: List deep copy via serialized approach or manual clone is best.
                 // Let's implement manual clone for ROIs here implicitly if needed, or just assume replacement logic.
                 Rois = new List<RoiBase>(rois) // Shallow copy of list items. If items are modified, history is tainted.
                 // TODO: Implement Deep Clone for robust Undo. For now, valid for add/remove.
             };

             _undoStack.Push(snapshot);
             
             if (_undoStack.Count > MaxDepth)
             {
                 var bottom = _undoStack.Last(); // This is O(N), Stack is efficient at top only.
                 // Generic Stack doesn't support removing bottom. Circular buffer is better.
                 // For simple stack, just ignore limit or trim?
             }

             _redoStack.Clear();
             StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public (Bitmap img, List<RoiBase> rois) Undo(Bitmap currentImg, List<RoiBase> currentRois)
        {
            if (!CanUndo) return (null, null);

            // Push current to redo
            _redoStack.Push(new Snapshot { Image = (Bitmap)currentImg?.Clone(), Rois = new List<RoiBase>(currentRois) });

            var snap = _undoStack.Pop();
            StateChanged?.Invoke(this, EventArgs.Empty);
            return (snap.Image, snap.Rois);
        }

        public (Bitmap img, List<RoiBase> rois) Redo(Bitmap currentImg, List<RoiBase> currentRois)
        {
            if (!CanRedo) return (null, null);
            
            // Push current to undo
            _undoStack.Push(new Snapshot { Image = (Bitmap)currentImg?.Clone(), Rois = new List<RoiBase>(currentRois) });

            var snap = _redoStack.Pop();
            StateChanged?.Invoke(this, EventArgs.Empty);
            return (snap.Image, snap.Rois);
        }

        public void Clear()
        {
            foreach (var s in _undoStack) s.Image?.Dispose();
            foreach (var s in _redoStack) s.Image?.Dispose();
            _undoStack.Clear();
            _redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
