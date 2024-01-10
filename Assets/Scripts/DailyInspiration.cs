using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizenApp
{
    public class DailyInspiration : MonoBehaviour
    {
        private const string COLUMN1 = "lbl_column1";
        private const string COLUMN2 = "lbl_column2";
        private const string COLUMN3 = "lbl_column3";
        private const string COLUMN4 = "lbl_column4";
        private const string DAY = "lbl_day";

        public const string INSPIRATION_PAGE_CLICKED = "InspirationPageClicked";


        [SerializeField] TextAsset InspirationsFile;

        //import tab delimited file into a dictionary with int key and string value
        Dictionary<int, string> Inspirations = new Dictionary<int, string>();
        //add to dictionary from text file
        private string _day;
        private string _inspiration;

        [SerializeField] private UIDocument _UIDocument;
        private VisualElement _root;
        private Label _dayLabel;
        private Label _column1;
        private Label _column2;
        private Label _column3;
        private Label _column4;

        private void Awake()
        {
            _root = _UIDocument.rootVisualElement;
            
        }

        private void Start()
        {
            InitializeDailyInspirationPage();
        }

        private void InitializeDailyInspirationPage()
        {
            Debug.Log("Daily Inspiration Page Geometry Changed");
            BindElements();
            ClearPlaceholderText();
            //get date in month day format
            _day = System.DateTime.Now.ToString("dd");
            //convert date format to words

            Debug.Log(_day);
            MakeInspirationDictionary();
            _inspiration = GetInspiration();
            SetInspirationTextFields(_inspiration);
            SetDayLabel(_day);
            _UIDocument.rootVisualElement.RegisterCallback<PointerDownEvent>(OnPageClicked);
        }

        private void ClearPlaceholderText()
        {
            _column1.text = "";
            _column2.text = "";
            _column3.text = "";
            _column4.text = "";
        }

        
        private void BindElements()
        {
            _column1 = _root.Q<Label>(COLUMN1);
            _column2 = _root.Q<Label>(COLUMN2);
            _column3 = _root.Q<Label>(COLUMN3);
            _column4 = _root.Q<Label>(COLUMN4);
            _dayLabel = _root.Q<Label>(DAY);

        }

        private void MakeInspirationDictionary()
        {
            string[] InspirationsLines = InspirationsFile.text.Split('\n');
            foreach (string line in InspirationsLines)
            {
                string[] splitLine = line.Split('\t');
                Inspirations.Add(int.Parse(splitLine[0]), splitLine[1]);
                //Debug.Log(splitLine[0] + " " + splitLine[1]);
            }
        }

        private string GetInspiration()
        {
            return Inspirations[int.Parse(_day)];
        }

        private void SetInspirationTextFields(string inspiration)
        {
            if(_column1 == null || _column2 == null || _column3 == null)
            {
                Debug.LogError("Daily Inspiration Text Fields Not Bound");
                return;
            }   
            //split string if contains spaces
            string[] splitInspiration = inspiration.Split(' ');
            //set text fields
            for (int i = 0; i < splitInspiration.Length; i++)
            {
                Debug.Log(splitInspiration[i]);
                if(i == 0)
                {
                    _column1.text = splitInspiration[i];
                }
                else if (i == 1)
                {
                    _column2.text = splitInspiration[i];
                }
                else if (i == 2)
                {
                    _column3.text = splitInspiration[i];
                }
                else if (i == 3)
                {
                    _column4.text = splitInspiration[i];
                }
            }
        }

        private void SetDayLabel(string day)
        {
            _dayLabel.text = day;
        }

        private void OnPageClicked(PointerDownEvent evt)
        {
            Debug.Log("Daily Inspiration Page Clicked");
            EventManager.TriggerEvent(INSPIRATION_PAGE_CLICKED, null);
        }
       
    }

}
