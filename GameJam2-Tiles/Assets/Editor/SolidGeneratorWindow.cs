using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;

public class FileUIElement
{
    public FileInfo file;
    public string label;
    public bool selected;
    public string extension;
    public string group = "";
    public List<string> tagList;

    public FileUIElement(FileInfo file, string extension)
    {
        this.file = file;
        this.extension = extension;
        label = file.Name.Replace(extension, "");
    }

    public static List<List<FileUIElement>> Group (List<FileUIElement> list)
    {
        List<string> groupNames = new List<string>();
        List<List<FileUIElement>> groupedLists = new List<List<FileUIElement>>();

        foreach(FileUIElement x in list)
        {
            string groupName = x.group;
            int id = groupNames.IndexOf(groupName);

            if(id == -1)
            {
                groupNames.Add(groupName);
                groupedLists.Add(new List<FileUIElement>());
                id = groupNames.Count - 1;
            }
            groupedLists[id].Add(x);
        }
        return groupedLists.Where(x => x.Count > 0).ToList();
    }
}

public class SelectionPanel
{
    public Vector2 scrollPos;
    public DirectoryInfo directory;
    public List<FileUIElement> view;
    public List<FileUIElement> folder;
    public List<FileUIElement> search;
    public List<FileUIElement> selected;
    public string name;
    public string rootPath;
    public string extension;
    public string searchbar;
    public string changed_searchbar;
    public bool refreshFolder;
    public bool refreshSearch;
    public bool refreshView;
    public float width;


    public SelectionPanel(string name, string rootPath, string extension, float width)
    {
        this.name = name;
        this.rootPath = rootPath;
        this.extension = extension;
        this.width = width;

        searchbar = "";

        view = new List<FileUIElement>();
        folder = new List<FileUIElement>();
        search = new List<FileUIElement>();
        selected = new List<FileUIElement>();

        scrollPos = Vector3.zero;
        
    }

    public string[] files
    {
        get
        {
            return selected.Select(x => x.file.FullName).ToArray();
        }
    }

    public void RefreshFolder()
    {
        refreshFolder = false;

        directory = new DirectoryInfo(string.Format(@"{0}{1}",rootPath, name));

        if (directory.Exists)
        {
            folder.Clear();
            FileInfo[] fileInfos = directory.GetFiles("*"+extension, SearchOption.TopDirectoryOnly);
            folder = fileInfos.Select(x => new FileUIElement(x, extension)).ToList();
            RefreshSearch();
        }
    }

    private void RefreshSearch()
    {
        refreshSearch = false;

        if (searchbar == "")
        {
            search = folder;
        }
        else
        {
            // Get user search tags
            DirectoryInfo cDir = new DirectoryInfo(string.Format(@"{0}{1}", rootPath, "Solid Classification"));
            List<string> userSearchTags = searchbar.Split(' ').ToList();

            // Get classification files that match user search tags
            List<FileInfo> fileInfos = new List<FileInfo>();
            userSearchTags.ForEach(tag => fileInfos.AddRange(cDir.GetFiles("*.txt", SearchOption.AllDirectories)
                .Where(file => file.Name.Replace(".txt","").ToLower().Equals(tag.ToLower()))));

            // Get names in classes
            List<string> searchQueries = new List<string>();
            searchQueries.AddRange(userSearchTags);
            fileInfos.ForEach(x => searchQueries.AddRange(ReadSolidClassFile(x)));

            // Get results from all queries
            List<FileUIElement> searchResults = new List<FileUIElement>();
            searchQueries.ForEach(query => searchResults.AddRange(folder
                .Where(entry => entry.label.ToLower().Equals(query.ToLower())).ToList()));
            search = searchResults;
        }

 
        RefreshView();
    }

    private void RefreshView()
    {
        refreshView = false;

        List<FileUIElement> changed_view = new List<FileUIElement>();
        changed_view.AddRange(selected.Where(x => !search.Contains(x)));
        changed_view.AddRange(search);

        view = changed_view;
    }

    private List<string> ReadSolidClassFile(FileInfo file)
    {
        string line;
        List<string> names = new List<string>();
        StreamReader reader = new StreamReader(file.FullName, Encoding.Default);
        SolidFileBuffer buffer = new SolidFileBuffer();

        using (reader)
        {
            while ((line = reader.ReadLine()) != null)
            {
                names.Add(line);
            }
        }

        return names;
    }


    public void DrawPanel(GUIStylePack stylePack)
    {
        GUILayout.BeginVertical(GUILayout.MinWidth(width));
        GUILayout.Label(name, EditorStyles.boldLabel);
        GUILayout.Label("");

        //Search
        GUILayout.BeginHorizontal();
        changed_searchbar = EditorGUILayout.TextField(searchbar);
        refreshFolder = GUILayout.Button("Refresh") || directory == null;
        GUILayout.EndHorizontal();


        //Validate
        refreshSearch = !changed_searchbar.Equals(searchbar);

        if (refreshFolder)
        {
            RefreshFolder();
        }
        else if (refreshSearch)
        {
            searchbar = changed_searchbar;
            RefreshSearch();
        }

        // Selection Total
        GUILayout.BeginHorizontal();
        bool all = GUILayout.Button("Select All");
        bool none = GUILayout.Button("Deselect All");
        if (all || none)
        {
            view.ForEach(x => x.selected = all);
            if (all)
            {
                // Add all visible unselected elements
                selected.AddRange(view.Where(x => !selected.Contains(x)));
            }
            else if (none)
            {
                // Remove all visible selected elements
                view.Where(x => selected.Contains(x)).ToList().ForEach(x => selected.Remove(x));
            }
            refreshFolder = true;
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("");

        // Selection List
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        //Debug.Log(string.Format("{0} ({1})", name, view.Count));
        if (view.Count > 0)
        {
            FileUIElement.Group(view).ForEach(x => DrawFileGroup(x, stylePack));
        }
        else
        {
            GUILayout.Label("");
            GUILayout.Label("");
            GUILayout.Label("       No matching solids found.");
            GUILayout.Label("");
            GUILayout.Label("");
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();        
    }

    void DrawFileGroup(List<FileUIElement> list, GUIStylePack stylePack)
    {
        string groupName = list[0].group == "" ? "Group" : list[0].group;
        GUI.color = stylePack.colHeader;
        GUILayout.Label(string.Format("{0}({1})",groupName,list.Count));
        GUI.color = stylePack.colDefault;

        for (int i = 0; i < list.Count; i++)
        {
            FileUIElement button = view[i];
            GUI.color = button.selected ? stylePack.colHighlight : stylePack.colDefault;

            if (GUILayout.Button(button.label))
            {
                refreshView = true;
                if (button.selected)
                {
                    button.selected = false;
                    selected.Remove(button);
                }
                else
                {
                    button.selected = true;
                    selected.Add(button);
                }
            }
            GUI.color = stylePack.colDefault;
        }
    }
}

public struct GUIStylePack
{
    public Color colDefault;
    public Color colInactive;
    public Color colHighlight;
    public Color colHeader;
}


public class SolidGeneratorWindow : EditorWindow
{
    GUIStylePack stylePack;

    public List<SelectionPanel> selectionPanels;
    public static SolidGeneratorWindow window;
    public string solidCollectionName;

    [MenuItem("Window/Experimental Game Design/SolidGenerator")]
    static void Init()
    {
        if (!window)
        {
            window = (SolidGeneratorWindow)GetWindow(typeof(SolidGeneratorWindow));
        }
        window.stylePack = new GUIStylePack
        {
            colDefault = GUI.color,
            colInactive = Color.Lerp(GUI.color, Color.black, 0.5f),
            colHighlight = Color.Lerp(GUI.color, Color.green, 0.5f),
            colHeader = Color.white
        };

        window.selectionPanels = new List<SelectionPanel>()
        {
            new SelectionPanel("Solid Data Raw", "Assets/Resources/", ".txt", 450),
            new SelectionPanel("Solid Collections", "Assets/Resources/", ".asset", 200)
        };

        window.Show();

    }


    void OnGUI()
    {
        if (selectionPanels == null || selectionPanels.Contains(null) || !window)
        {
            Init();
        }

        if (!selectionPanels.Contains(null))
        {
            SelectionPanel resources = selectionPanels[0];
            SelectionPanel assets = selectionPanels[1];

            // Generator 
            GUILayout.Label("");
            GUILayout.Label("Save Selected Resources as Solid Collection", EditorStyles.boldLabel);
            bool generateCollection;
            GUILayout.BeginHorizontal();
            solidCollectionName = EditorGUILayout.TextField(solidCollectionName);
            generateCollection = GUILayout.Button("Gernerate") && resources.selected.Count > 0;
            GUILayout.EndHorizontal();
            GUILayout.Label("");

            if (generateCollection)
            {
                SolidCollection solidCollection;
                string assetPath;

                // Create asset
                solidCollection = CreateInstance<SolidCollection>();
                solidCollection.name = solidCollectionName;
                assetPath = string.Format(@"{0}\{1}.{2}", "Assets/Resources/Solid Collections", solidCollectionName, "asset");
                AssetDatabase.CreateAsset(solidCollection, assetPath);
                solidCollection.CreateSolids(resources.files);
                try
                {
                    AssetDatabase.SaveAssets();
                }
                catch (NullReferenceException e)
                {
                    Debug.Log(e.HelpLink);
                }

                assets.refreshFolder = true;
            }


            // SelectionPanels 
            GUILayout.BeginHorizontal();
            //selectionPanels.ForEach(x => x.DrawPanel(stylePack));
            resources.DrawPanel(stylePack);
            GUILayout.EndHorizontal();
            GUILayout.Label("");
        }
    }
}