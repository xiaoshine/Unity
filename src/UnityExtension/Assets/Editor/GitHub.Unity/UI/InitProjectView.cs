using System;
using UnityEditor;
using UnityEngine;

namespace GitHub.Unity
{
    [Serializable]
    class InitProjectView : Subview
    {
        private const string NoRepoTitle = "To begin using GitHub, initialize a git repository";
        private const string NoUserOrEmailError = "Name and email not set in git. Go into the settings tab and enter the missing information";

        [NonSerialized] private bool isBusy;

        [NonSerialized] private bool isUserDataPresent;
        [NonSerialized] private bool userDataHasChanged;

        public override void InitializeView(IView parent)
        {
            base.InitializeView(parent);

            if (!string.IsNullOrEmpty(Environment.GitExecutablePath))
            {
                CheckForUser();
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            userDataHasChanged = Environment.GitExecutablePath != null;
        }

        public override void OnGUI()
        {
            GUILayout.BeginVertical(Styles.GenericBoxStyle);
            {
                GUILayout.FlexibleSpace();
                GUILayout.Space(-140);

                GUILayout.BeginHorizontal();
                {
                  GUILayout.FlexibleSpace();
                  GUILayout.Label(Styles.EmptyStateInit, GUILayout.MaxWidth(265), GUILayout.MaxHeight(136));
                  GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(NoRepoTitle, Styles.BoldCenteredLabel);
                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginDisabledGroup(IsBusy || !isUserDataPresent);
                    {
                        if (GUILayout.Button(Localization.InitializeRepositoryButtonText, "Button"))
                        {
                            isBusy = true;
                            Manager.InitializeRepository()
                                   .FinallyInUI(() => isBusy = false)
                                   .Start();
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                if (!isUserDataPresent)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(NoUserOrEmailError, MessageType.Error);
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }

        private void MaybeUpdateData()
        {
            if (userDataHasChanged)
            {
                userDataHasChanged = false;
                CheckForUser();
            }
        }

        private void CheckForUser()
        {
            isBusy = true;

            GitClient.GetConfigUserAndEmail().FinallyInUI((success, ex, strings) => {
                var username = strings[0];
                var email = strings[1];

                isBusy = false;
                isUserDataPresent = success && !String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(email);

                Logger.Trace("Finally: {0}", isUserDataPresent);

                Redraw();
            }).Start();
        }

        public override bool IsBusy
        {
            get { return isBusy;  }
        }
    }
}
