
# Unity UI Builder Extensions

![UIBuilderExtensions](https://user-images.githubusercontent.com/8216996/88447895-31c86900-ce73-11ea-9809-73a3c26bd2b8.gif)

# Requirements

* Unity 2019.4 or later
* UI Builder

# How to install
Add a dependency into your manifest.json.
```
{
  "dependencies": {
    "com.t-macnaga.ui-builder-extensions": "https://github.com/t-macnaga/UIBuilderExtensions.git",
  }
}
```

# What is this?

* Editor C# Code Generation
    * If you open Events, you can use the following methods: OnClick for Buttons, OnValueChanged for TextField and Toggle, and so on.
In the event list, double-click on the callback you want to add, and you'll see the
Create C# EditorWindow code at the same level as the UXML