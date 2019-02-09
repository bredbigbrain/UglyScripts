using UnityEditor;
using UnityEngine;

namespace Ugly.BuildPreprocess
{
    /// <summary>
    /// POT textures check.
    /// Проверка POT текстур.
    /// </summary>
    public class ExamplePreprocessor : AbstractPreprocessor
    {
        //Additional parameters (optional).
        //Дополнительные параметры (опционально).
        public class Config: DefaultConfig
        {
            public int maxSize = 1024;
        }

        private Config config;
        private Texture2D[] textures;

        //If additional parameters and assets are not used : base(typeof(DefaultConfig), false).
        //Если дополнительные параметны не нужны и ассеты тоже : base(typeof(DefaultConfig), false).
        public ExamplePreprocessor() : base(typeof(Config), true) { }

        //Override if additional parameters are used.
        //Если используются дополнительные параметры.
        protected override string GetJsonConfig()
        {
            return JsonUtility.ToJson(config);
        }

        protected override void Initialize()
        {
            config = defaultConfig as Config;
            textures = GetAssets<Texture2D>();
        }

        protected override bool Process(out string message)
        {
            bool isOK = true;
            message = null;
            for (int i = 0; i < textures.Length; i++)
            {
                string str;
                if (textures[i].height == textures[i].width)
                {
                    str = textures[i].name + " is POT";
                }
                else
                {
                    str = textures[i].name + " is NOT POT";
                    isOK = false;
                }
                message = message == null ? str : message + "\n" + str;

                if (textures[i].height > config.maxSize || textures[i].width > config.maxSize)
                {
                    message += ", TOO BIG";
                    isOK = false;
                }
                else
                {
                    message += ", size is ok";
                }
            }
            return isOK;
        }

        public override string GetName()
        {
            //You can change the name. Default name is type name (here it is "ExamplePreprocessor").
            //Можно изменить название. По дефолту название типа (тут это "ExamplePreprocessor").
            return base.GetName();
        }

        public override void InitGUI()
        {
            //This called before config window opens. Something heavy used for draw GUI can be placed here.
            //Вызывается перед открытием окна настройки конфига. Можно поместить сюда что-нибудь тяжелое, необходимое для отрисовки GUI.
        }

        public override void OnGUI()
        {
            config.maxSize = EditorGUILayout.IntField("max size", config.maxSize);

            GUILayout.Label("Textures");
            DrawAssetsList<Texture2D>();
        }
    }
}
