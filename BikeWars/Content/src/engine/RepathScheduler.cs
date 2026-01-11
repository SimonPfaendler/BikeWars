using BikeWars.Content.engine;

public sealed class RepathScheduler
{
    private readonly EnemyMovement[] _buffer;
    private int _head = 0;
    private int _tail = 0; 
    private int _count = 0;
    public int UpdateMaxEnemies { get; set; }

    public RepathScheduler(int capacity)
    {
        _buffer = new EnemyMovement[capacity];
    }

    public bool Request(EnemyMovement enemy)
    {
        if (enemy == null)
            return false;

        if (enemy.IsRepathQueued)
            return false;
        
        if (_count == _buffer.Length)
            return false;

        _buffer[_tail] = enemy;
        _tail = (_tail + 1) % _buffer.Length;
        _count++;
        
        enemy.IsRepathQueued = true;
        return true;
    }

    public void Update()
    {
        int processed = 0;

        while (processed < UpdateMaxEnemies && _count > 0)
        {
            var enemy =  _buffer[_head];
            _buffer[_head] = null;
            
            _head = (_head + 1) % _buffer.Length;
            _count--;
            
            if (enemy != null)
            {
                enemy.IsRepathQueued = false;
                enemy.DoRepathNow();
            }

            processed++;
        }
    }
}