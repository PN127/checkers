using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour, IForwardObserver
    {
        public static GameManager gm;
        IObserver obs;
        public Material green;
        public Material yellow;

        private string _alphabet, _nameChipB, _nameChipW;
        private static CellComponent[] choiceCell = new CellComponent[2];
        
        private GameObject selectedFigure;
        private GameObject _targetChip;
        private GameObject _eventSystem;

        private byte _countW;
        private byte _countB;
        private bool _lockInput;

        [SerializeField, Range(1f, 100f)]
        private int _rotationSpeed = 20;
        [SerializeField, Range(1f, 10f)]
        private float _speed = 1;

        [SerializeField]
        private GameObject _cemeteryWhite;
        [SerializeField]
        private GameObject _cemeteryBlack;

        [SerializeField]
        private bool stepBlack;
        
        [Header("Сложность игры"), SerializeField]
        private DifficultyType Difficulty;

        private void Awake()
        {
            gm = this;
            _alphabet = "ABCDEFGH";
            _nameChipB = "ChipB";
            _nameChipW = "ChipW";
        }

        private void Start()
        {
            stepBlack = false;
            ConfigurationPair();
            _eventSystem = GameObject.Find("EventSystem");
            if (gm.GetComponent<Observer>() != null)
                obs = gm.GetComponent<Observer>();
        }

        private void ConfigurationPair()
        {
            byte numberChip = 1;
            byte numberLetter = 0;
            char startLetter = _alphabet[numberLetter];
            GameObject cell;
            GameObject chip;

            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cell = GameObject.Find(Convert.ToString(startLetter) + Convert.ToString(i));
                    chip = GameObject.Find(_nameChipB + Convert.ToString(numberChip));

                    cell.GetComponent<CellComponent>().OnClickEventHandler += CellOnClickEventHandler;
                    cell.GetComponent<CellComponent>().OnFocusEventHandler += CellOnFocusEventHandler;

                    chip.GetComponent<ChipComponent>().OnClickEventHandler += ChipOnClickEventHandler;
                    chip.GetComponent<ChipComponent>().OnTriggerEnterEventHandler += ChipOnTriggerEnterEventHandler;

                    cell.GetComponent<CellComponent>().Pair = chip.GetComponent<ChipComponent>();
                    chip.GetComponent<ChipComponent>().Pair = cell.GetComponent<CellComponent>();

                    numberChip++;
                    numberLetter += 2;
                    if (numberLetter > 7) continue;
                    startLetter = _alphabet[numberLetter];
                }

                if (numberLetter == 8)
                    numberLetter = 1;
                if (numberLetter == 9)
                    numberLetter = 0;
                startLetter = _alphabet[numberLetter];
            }

            numberChip = 1;
            numberLetter = 1;
            startLetter = _alphabet[numberLetter];
            
            for (int i = 8; i > 5; i--)
            {
                for (int j = 0; j < 4; j++)
                {
                    cell = GameObject.Find(Convert.ToString(startLetter) + Convert.ToString(i));
                    chip = GameObject.Find(_nameChipW + Convert.ToString(numberChip));

                    cell.GetComponent<CellComponent>().OnClickEventHandler += CellOnClickEventHandler;
                    cell.GetComponent<CellComponent>().OnFocusEventHandler += CellOnFocusEventHandler;

                    chip.GetComponent<ChipComponent>().OnClickEventHandler += ChipOnClickEventHandler;
                    chip.GetComponent<ChipComponent>().OnTriggerEnterEventHandler += ChipOnTriggerEnterEventHandler;

                    cell.GetComponent<CellComponent>().Pair = chip.GetComponent<ChipComponent>();
                    chip.GetComponent<ChipComponent>().Pair = cell.GetComponent<CellComponent>();

                    numberChip++;
                    numberLetter += 2;
                    if (numberLetter > 7) continue;
                    startLetter = _alphabet[numberLetter];
                }

                if (numberLetter == 8)
                    numberLetter = 1;
                if (numberLetter == 9)
                    numberLetter = 0;
                startLetter = _alphabet[numberLetter];

            }
                        
            for (int i = 4; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    startLetter = _alphabet[j];

                    cell = GameObject.Find(Convert.ToString(startLetter) + Convert.ToString(i));

                    cell.GetComponent<CellComponent>().OnClickEventHandler += CellOnClickEventHandler;
                    cell.GetComponent<CellComponent>().OnFocusEventHandler += CellOnFocusEventHandler;
                }
            }
        }

        #region Cell

        private void CellOnFocusEventHandler(CellComponent component, bool isSelect)
        {
            if (_lockInput) return;

            if (isSelect)
            {
                if (CheckPairColor(out byte b, component) != 3) return;

                if (Difficulty == DifficultyType.Hard)
                    component.InstallMaterials(1);

                if (Difficulty == DifficultyType.Easy)
                {
                    CellNeighbors((CellComponent)component, component.Pair, out var left, out var right);
                    if ((left != null && left.Pair == null) || (right != null && right.Pair == null))
                    {
                        component.InstallMaterials(1);
                        return;
                    }
                    if (left != null && left.Pair != null)
                    {
                        CellNeighbors(left, component.Pair, out var left_Left, out var left_Right);
                        if (((left.Pair.GetColor != component.Pair.GetColor) && (left_Left != null) && (left_Left.Pair == null)))
                        {
                            component.InstallMaterials(1);
                            return;
                        }
                    }
                    if (right != null && right.Pair != null)
                    {
                        CellNeighbors(right, component.Pair, out var rilght_Left, out var right_Right);
                        if (((right.Pair.GetColor != component.Pair.GetColor) && (right_Right != null) && (right_Right.Pair == null)))
                        {
                            component.InstallMaterials(1);
                            return;
                        }
                    }
                }
            }
            if (!isSelect)
            {               
                if (component.CleaningName(component))
                    component.InstallMaterials(0);
            }
        }

        private void CellOnClickEventHandler(BaseClickComponent component)
        {
            if (_lockInput) return;

            if ((component.Pair == null) && ((choiceCell[0] == component) || (choiceCell[1] == component)))
            {
                if ((selectedFigure.GetComponent<ChipComponent>().GetColor == ColorType.White) && !stepBlack)
                    StartCoroutine(FigureMovement(selectedFigure, component.gameObject));
                if ((selectedFigure.GetComponent<ChipComponent>().GetColor == ColorType.Black) && stepBlack)
                    StartCoroutine(FigureMovement(selectedFigure, component.gameObject));

                _targetChip = null;

                selectedFigure.GetComponent<ChipComponent>().Pair.Pair = null;
                selectedFigure.GetComponent<ChipComponent>().Pair = component;

                CellCleaner();

                component.Pair = selectedFigure.GetComponent<ChipComponent>();
                component.Pair.InstallMaterials();
            }
            else if (component.Pair != null)
                ChipOnClickEventHandler(component.Pair);
            if (!obs.Read)
                Write("Move", component.name);
        }

        #endregion

        #region Chip

        private void ChipOnClickEventHandler(BaseClickComponent component)
        {
            if (_lockInput) return;

            if (selectedFigure != null)
            {
                selectedFigure.GetComponent<ChipComponent>().InstallMaterials();
                selectedFigure = null;
                CellCleaner();
            }

            CheckPairColor( out byte b, (CellComponent)component.Pair);
            switch (b)
            {
                case 0:
                    return;
                case 1:
                    Debug.LogWarning("Сейчас ход белых");
                    return;
                case 2:
                    Debug.LogWarning("Сейчас ход черных");
                    return;
            }

            component.InstallMaterials(1);

            if (component.GetColor == ColorType.Black)
            {
                var TopLeft = component.Pair.GetComponent<CellComponent>().GetNeighbors(NeighborType.TopLeft);
                var TopRight = component.Pair.GetComponent<CellComponent>().GetNeighbors(NeighborType.TopRight);

                if (TopLeft != null)
                    ChoiceMove(TopLeft, 0, NeighborType.TopLeft);
                if (TopRight != null)
                    ChoiceMove(TopRight, 1, NeighborType.TopRight);
            }
            else
            {
                var BottomLeft = component.Pair.GetComponent<CellComponent>().GetNeighbors(NeighborType.BottomLeft);
                var BottomRight = component.Pair.GetComponent<CellComponent>().GetNeighbors(NeighborType.BottomRight);

                if (BottomLeft != null)
                    ChoiceMove(BottomLeft, 0, NeighborType.BottomLeft);
                if (BottomRight != null)
                    ChoiceMove(BottomRight, 1, NeighborType.BottomRight);

            }
            selectedFigure = component.gameObject;
            if (!obs.Read)
                Write("Chip", component.Pair.name);

        }

        private void ChipOnTriggerEnterEventHandler(Collider other)
        {
            if (other.GetComponent<CellComponent>()) return;
            var  other_Component = other.gameObject.GetComponent<ChipComponent>();
            CheckPairColor(out byte b, null, other_Component);
            if (b == 0 || b == 3) return;
            other.enabled = false;
            if (!obs.Read)
                Write("Destroy", other_Component.Pair.name);
            KillChip(other.gameObject);
            
        }

        #endregion

        #region Methods
                
        private IEnumerator FigureMovement(GameObject figure, GameObject destination, bool kill = false)
        {
            _lockInput = true;

            if (!kill)
            {
                var point = destination.transform.position + new Vector3(0, 0.1f, 0);
                figure.transform.LookAt(point);
                var distance = Vector3.Distance(figure.transform.position, point);

                while (distance > 0)
                {
                    figure.transform.position += figure.transform.TransformDirection(Vector3.forward) * Time.deltaTime * _speed;
                    distance -= Time.deltaTime * _speed;
                    yield return null;
                }
                figure.transform.position = point;

                GameOver(figure.GetComponent<ChipComponent>());
                StartCoroutine(CameraMovement(_eventSystem));
                
            }

            if(kill)
            {
                var point = destination.transform.position + new Vector3(0, 0.1f, 0);

                while (figure.transform.position != point)
                {
                    figure.transform.position = Vector3.Lerp(figure.transform.position, point, 0.1f);
                    yield return null;
                }
                
            }            
            
            StopCoroutine(FigureMovement(figure, destination));
        }

        private IEnumerator CameraMovement(GameObject _eventSystem)
        {
            float i = 0;
            while (i < 180)
            {
                _eventSystem.transform.eulerAngles += Vector3.up * Time.deltaTime * 1.3f * _rotationSpeed;
                i += Time.deltaTime * 1.3f * _rotationSpeed;
                yield return null;
            }
            if (_eventSystem.transform.eulerAngles.y > 170)
                _eventSystem.transform.eulerAngles = new Vector3(0, 180, 0);
            if (_eventSystem.transform.eulerAngles.y < 10)
                _eventSystem.transform.eulerAngles = new Vector3(0, 0, 0);
            if (!obs.Read)
                stepBlack = !stepBlack;

            StopCoroutine(CameraMovement(_eventSystem));
            _lockInput = false;
        }

        private void KillChip(GameObject corpse)
        {
            var corpseComponent = corpse.GetComponent<ChipComponent>();
            corpseComponent.Pair.Pair = null;
            corpseComponent.Pair = null;

            if (corpseComponent.GetColor == ColorType.White)
            {
                StartCoroutine(FigureMovement(corpse, _cemeteryWhite, true));                
                _cemeteryWhite.transform.position += _cemeteryWhite.transform.TransformDirection(Vector3.forward);
                _countB++;
                if (_countB == 6)
                    _cemeteryWhite.transform.position = new Vector3(10.5f, 0, 1.5f);
            }
            if (corpseComponent.GetColor == ColorType.Black)
            {
                StartCoroutine(FigureMovement(corpse, _cemeteryBlack, true));
                _cemeteryBlack.transform.position += _cemeteryBlack.transform.TransformDirection(Vector3.forward);
                _countW++;
                if (_countW == 6)
                    _cemeteryBlack.transform.position = new Vector3(-1.5f, 0, 7.5f);
            }
        }

        private void GameOver(ChipComponent chip)
        {
            if ((_countB == 12) || (chip.GetColor == ColorType.Black && chip.Pair.name[1] == '8'))
            {
                Debug.Log("---!!! Black WiNNER !!!---");
                if (!obs.Read)
                    Write("Win", "I am WINNER!");
                UnityEditor.EditorApplication.isPaused = true;
            }
            if ((_countW == 12) || (chip.GetColor == ColorType.White && chip.Pair.name[1] == '1'))
            {
                Debug.Log("---!!! White WiNNER !!!---");
                if (!obs.Read)
                    Write("Win", "I am WINNER!");
                UnityEditor.EditorApplication.isPaused = true;
            }

            //if (chip.GetColor == ColorType.Black && chip.Pair.name[1] == '8')
            //{
            //    Debug.Log("---!!! Black WiNNER !!!---");
            //    UnityEditor.EditorApplication.isPaused = true;
            //}
            //if (chip.GetColor == ColorType.White && chip.Pair.name[1] == '1')
            //{
            //    Debug.Log("---!!! White WiNNER !!!---");
            //    UnityEditor.EditorApplication.isPaused = true;
            //}
        }

        private byte CheckPairColor(out byte b, CellComponent component = null, ChipComponent chip = null)
        {
            if (component != null)
            {
                if (component.Pair == null) return b = 0;
                if ((component.Pair.GetColor == ColorType.Black) && !stepBlack) return b = 1;
                if ((component.Pair.GetColor == ColorType.White) && stepBlack) return b = 2;
                return b = 3;
            }
            else
            {
                if (chip == null) return b = 0;
                if ((chip.GetColor == ColorType.Black) && !stepBlack) return b = 1;
                if ((chip.GetColor == ColorType.White) && stepBlack) return b = 2;
                return b = 3;
            }
        }

        private void CellCleaner()
        {
            if (choiceCell[0] != null)
                choiceCell[0].InstallMaterials();
            if (choiceCell[1] != null)
                choiceCell[1].InstallMaterials();
            choiceCell[0] = null;
            choiceCell[1] = null;
        }

        private void ChoiceMove(CellComponent component, byte i, NeighborType direction)
        {
            if (component.Pair != null)
            {
                CheckPairColor(out byte b, component);
                if (b == 0 || b == 3) return;

                var neighbor = component.GetNeighbors(direction);
                if (neighbor == null) return;
                if (neighbor.Pair != null) return;
                neighbor.InstallMaterials(2);
                choiceCell[i] = neighbor;
                if (_targetChip == null)
                    _targetChip = component.Pair.gameObject;
            }
            else
            {
                component.InstallMaterials(2);
                choiceCell[i] = component;
            }
        }

        private void CellNeighbors(CellComponent component, BaseClickComponent pair, out CellComponent left, out CellComponent right)
        {
            if (pair.GetColor == ColorType.Black)
            {
                left = component.GetNeighbors(NeighborType.TopLeft);
                right = component.GetNeighbors(NeighborType.TopRight);
            }
            else
            {
                left = component.GetNeighbors(NeighborType.BottomLeft);
                right = component.GetNeighbors(NeighborType.BottomRight);
            }
        }
        #endregion

        #region IObserverMethods

        public void Write(string b, string name)
        {
            string message = string.Empty;
            string p;
            if (!stepBlack)
                p = "Player 1:";
            else
                p = "Player 2:";

            switch (b)
            {
                case "Chip":
                    message = p + " Selected Chip " + name;
                    break;
                case "Move":
                    message = p + " Move to " + name;
                    break;
                case "Destroy":
                    message = p + " Destroy " + name;
                    break;
                case "Win":
                    message = p + name;
                    break;
            }
            obs.WriteInLog(message);
        }

        public void Reproduce(string a, string name)
        {
            BaseClickComponent bcc = null;
            switch (a)
            {
                case "Player 1":
                    stepBlack = false;
                    break;
                case "Player 2":
                    stepBlack = true;
                    break;
                case "Chip":
                    bcc = GameObject.Find(name).GetComponent<BaseClickComponent>().Pair;
                    ChipOnClickEventHandler(bcc);
                    break;
                case "Move":
                    bcc = GameObject.Find(name).GetComponent<BaseClickComponent>();
                    CellOnClickEventHandler(bcc);
                    break;
                case "Destroy":
                    
                    break;
                case "Win":
                    
                    break;
            }
        }
        #endregion

    }

    public enum DifficultyType
    {
        Easy,
        Hard
    }
}
