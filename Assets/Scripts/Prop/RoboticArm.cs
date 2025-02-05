﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamedevGBG.Prop
{
    public class RoboticArm : MonoBehaviour
    {
        [SerializeField]
        private AContainer _inputs, _outputs;

        [SerializeField]
        private float _speed;

        [SerializeField]
        private Transform _arm, _hand, _finger;

        private bool _isMoving;

        private int _targetIndex;
        private int _oldIndex;

        private float _oldDist;

        private float _baseValue;
        private float _objValue;
        private float _actionTimer;

        private const int _indexMaterialPreview = 2;
        private Material _defaultPreviewMaterial;

        [SerializeField]
        private MeshRenderer _previewMeshRenderer;

        private AudioSource _movementAudio;

        private PropInfo _propLoaded;

        private enum ActionState
        {
            GoDown,
            GoUp,
            Done
        }
        private ActionState _currentAction = ActionState.Done;

        private void Start()
        {
            _defaultPreviewMaterial = _previewMeshRenderer.materials[_indexMaterialPreview];
            _movementAudio = GetComponent<AudioSource>();
            _targetIndex = _inputs.TargetCount;
            _oldIndex = _targetIndex - 1;
            _oldDist = float.PositiveInfinity;
            _baseValue = _arm.transform.position.y;
            _objValue = _baseValue - .05f;
        }

        public void GoLeft()
        {
            if (_targetIndex > 0 && _currentAction == ActionState.Done)
            {
                _oldIndex = _targetIndex;
                _targetIndex--;
                _oldDist = float.PositiveInfinity;
            }
        }

        public void GoRight()
        {
            if (_targetIndex < _inputs.TargetCount + _outputs.TargetCount - 1 && _currentAction == ActionState.Done)
            {
                _oldIndex = _targetIndex;
                _targetIndex++;
                _oldDist = float.PositiveInfinity;
            }
        }

        public void DoAction()
        {
            if (!_isMoving && _currentAction == ActionState.Done
                &&
                (
                (_targetIndex < _inputs.TargetCount && !_inputs.IsEmpty(_targetIndex) && _propLoaded == null) ||
                (_targetIndex >= _inputs.TargetCount && !_outputs.IsEmpty(_targetIndex - _inputs.TargetCount) && _propLoaded != null && _outputs.GetPropInfo(_targetIndex - _inputs.TargetCount).Inside.Count < 4)
                ))
            {
                _currentAction = ActionState.GoDown;
                _actionTimer = 0f;
                _movementAudio.Play();
            }
        }

        private Vector3 GetPosition(int index)
        {
            if (index < _inputs.TargetCount)
            {
                return _inputs.GetPosition(index);
            }
            return _outputs.GetPosition(index - _inputs.TargetCount);
        }

        private void Update()
        {
            var target = GetPosition(_targetIndex);
            var dist = Vector3.Distance(_finger.transform.position, new Vector3(target.x, _finger.transform.position.y, target.z));
            if (dist < _oldDist)
            {
                _oldDist = dist;
                _arm.transform.position += _arm.transform.right * (_targetIndex > _oldIndex ? 1f : -1f) * _speed * Time.deltaTime;
                if (!_isMoving)
                {
                    _isMoving = true;
                    _movementAudio.Play();
                }
            }
            else
            {
                if (_isMoving)
                {
                    _isMoving = false;
                    _movementAudio.Stop();
                }
            }

            // Moving the arm up then down
            if (_currentAction == ActionState.GoDown)
            {
                _actionTimer += Time.deltaTime * 2f;
                _arm.position = new Vector3(
                    x: _arm.transform.position.x,
                    y: Mathf.Lerp(_baseValue, _objValue, _actionTimer),
                    z: _arm.transform.position.z
                    );
                if (_actionTimer >= 1f)
                {
                    _actionTimer = 0f;
                    _currentAction = ActionState.GoUp;
                    if (_targetIndex < _inputs.TargetCount) // Filling current
                    {
                        // Fill preview with new material
                        _propLoaded = _inputs.GetPropInfo(_targetIndex);
                        Material[] matArray = _previewMeshRenderer.materials;
                        matArray[_indexMaterialPreview] = _propLoaded.GetComponent<MeshRenderer>().materials[1];
                        _previewMeshRenderer.materials = matArray;
                    }
                    else
                    {
                        // Revert preview to default state
                        Material[] matArray = _previewMeshRenderer.materials;
                        matArray[_indexMaterialPreview] = _defaultPreviewMaterial;
                        _previewMeshRenderer.materials = matArray;

                        // Update vial color
                        var targetVial = _outputs.GetPropInfo(_targetIndex - _inputs.TargetCount);
                        var pi = targetVial.GetComponent<PropInfo>();
                        pi.Inside.Add(_propLoaded);
                        var vialMR = targetVial.GetComponent<MeshRenderer>();
                        matArray = vialMR.materials;
                        matArray[_orders[pi.Inside.Count - 1]] = _propLoaded.GetComponent<MeshRenderer>().materials[1];
                        vialMR.materials = matArray;
                        var id = targetVial.GetComponent<PropInfo>().ID;
                        List<string> elems;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            elems = id.Split(';').ToList();
                        }
                        else
                        {
                            elems = new();
                        }
                        elems.Add(_propLoaded.GetComponent<PropInfo>().ID);
                        targetVial.GetComponent<PropInfo>().ID = string.Join(";", elems);

                        _propLoaded = null;
                    }
                }
            }
            else if (_currentAction == ActionState.GoUp)
            {
                _actionTimer += Time.deltaTime * 2f;
                _arm.position = new Vector3(
                    x: _arm.transform.position.x,
                    y: Mathf.Lerp(_objValue, _baseValue, _actionTimer),
                    z: _arm.transform.position.z
                    );
                if (_actionTimer >= 1f)
                {
                    _actionTimer = 0f;
                    _currentAction = ActionState.Done;
                    _movementAudio.Stop();
                }
            }
        }

        private int[] _orders = new[]
        {
            1, 3, 4, 2
        };
    }
}
