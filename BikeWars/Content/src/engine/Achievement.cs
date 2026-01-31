using BikeWars.Content.managers;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.engine;
public class Achievement
{
    private AchievementIds _id { get; set; }
    public AchievementIds Id {
        get => _id;
        set
        {
            _id = value;
        }
    }
    private string _name { get; set; }
    public string Name {
        get => _name;
        set
        {
            _name = value;
        }
    }
    private string _description { get; set; }
    public string Description {
        get => _description;
        set
        {
            _description = value;
        }
    }

    private bool _succeeded { get; set;}
    public bool Succeeded {
        get => _succeeded;
        set
        {
            _succeeded = value;
        }
    }

    private Texture2D _picture { get; set;}
    public Texture2D Picture {
        get => _picture;
        set
        {
            _picture = value;
        }
    }


    public Achievement()
    {
        Id = 0;
        Name = "";
        Succeeded = false;
        Description = "";
        Picture = null;
    }

    public Achievement(AchievementIds id, string name, string description, bool succeded, Texture2D picture)
    {
        Id = id;
        Name = name;
        Description = description;
        Succeeded = succeded;
        Picture = picture;
    }
}