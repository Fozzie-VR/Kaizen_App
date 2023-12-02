using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizenApp
{
    //might want to use scriptable objects for this; likely to need a lot of types...
    public enum PageType
    {
        Landing,
        KaizenForm,
        PreKaizenLayout,
        PostKaizenLayout,
        Presentation
    }

    public enum  PageSortOrder
    {
        Front,
        Back
    }

    public enum PageState
    {
        Active,
        Inactive
    }

    public class PageStateModel
    {

    }

}
