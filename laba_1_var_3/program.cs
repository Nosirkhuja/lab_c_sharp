using System;

using System.Collections.Generic;

using System.Numerics;

using System.Collections;
using lab_1_var_3;

namespace lab_1_var_3
{

    struct DataItem
    {

        public double elec { get; set; }

        public Vector2 vec { get; set; }

        public DataItem(Vector2 v, double z)

        {
            elec = z;
            vec = v;
        }

        public override string ToString()

        {

            return $"elec={elec}; vec.ToString()";

        }
    }

    struct Grid1D

    {
        public float step { get; set; }

        public int count { get; set; }

        public Grid1D(float step_ = 0, int n_ = 10)

        {

            step = step_;

            count = n_;
        }

        public override string ToString()

        {

            return $"step={step}; count={count}";

        }
    }

    //абстрактный базовый класс

    abstract class V3Data

    {
        public string info { get; set; }

        public DateTime Dt { get; set; }

        public V3Data(string id, DateTime w)

        {
            info = id;

            Dt = w;
        }

        public abstract System.Numerics.Vector2[] Nearest(System.Numerics.Vector2 v);

        public abstract string ToLongString();

        public override string ToString()

        {

            return $"info={info}; dt={Dt}";

        }

    }

    class V3DataOnGrid : V3Data

    {
        public Grid1D grid_x { set; get; }

        public Grid1D grid_y { set; get; }

        public double[,] array { set; get; }

        public V3DataOnGrid(string id, DateTime w, Grid1D grid_x, Grid1D grid_y) : base(id, w)

        {
            Dt = w;

            info = id;

            array = new double[grid_x.count, grid_y.count];

        }

        public void InitRandom(double minValue, double maxValue)

        {

            //Создание объекта для генерации чисел

            Random random = new Random();

            for (int i = 0; i < grid_x.count; i++)

            {

                for (int j = 0; j < grid_y.count; j++)

                {

                    double x = random.NextDouble() * (maxValue - minValue) + minValue;

                    //Console.WriteLine(c);

                    array[i, j] = x;

                }

            }

        }

        public override System.Numerics.Vector2[] Nearest(Vector2 v)

        {

            Vector2[] res = new Vector2[0];

            int index = 0;

            Vector2 w = new System.Numerics.Vector2(0, 0);

            double min = Vector2.Distance(v,w );

            for (int i = 0; i < grid_x.count; i++)

            {

                for (int j = 0; j < grid_y.count; j++)

                {
                    Vector2 d = new System.Numerics.Vector2(i, j);
                    min = Math.Min(min, Vector2.Distance(v, d));

                }

            }

            for (int i = 0; i < grid_x.count; i++)

            {

                for (int j = 0; j < grid_y.count; j++)

                {

                    if (Vector2.Distance(v, new Vector2(i, j)) == min)

                    {

                        Array.Resize(ref res, index + 1);

                        res[index] = new System.Numerics.Vector2(i, j);

                        index++;

                    }

                }

            }

            return res;

        }

        public override string ToLongString()
        {
            string res = this.ToString();
            for (int i = 0; i < grid_x.count; i++)
            {
                for (int j = 0; j < grid_y.count; j++)
                {
                    res += "\n " + i.ToString() + ' ' + j.ToString() + ' ' + array[i, j].ToString();
                }
            }
            return res;
        }

        public override string ToString()

        {
            return "V3DataOnGrid " + base.ToString() + ' ' + grid_x.ToString() + ' ' + grid_y.ToString();
        }

    }

    class V3DataCollection : V3Data
    {
        public List<DataItem> lst { set; get; }

        public V3DataCollection(string id, DateTime dt) : base(id, dt)

        {
            lst = new List<DataItem>();
        }

        public static explicit operator V3DataCollection(V3DataOnGrid ld)
        {
            V3DataCollection res = new V3DataCollection(ld.info, ld.Dt);
            float step_x = ld.grid_x.step;
            float step_y = ld.grid_y.step;
            for (int i = 0; i < ld.grid_x.count; i++)
            {
                for (int j = 0; j < ld.grid_y.count; j++)
                {
                    res.lst.Add(new DataItem(new Vector2(i * step_x, j * step_y), ld.array[i, j]));
                }
            }
            return res;
        }

        public void InitRandom(int nItems, float xmax, float ymax, double minValue, double maxValue)
        {
            Random random = new Random();

            for (int i = 0; i < nItems; i++)

            {
                float x = (float)random.NextDouble() * xmax;

                float y = (float)random.NextDouble() * ymax;

                double z = (random.NextDouble() * (maxValue - minValue)) + minValue;

                lst.Add(new DataItem(new System.Numerics.Vector2(x, y),z));
            }
        }

        public override Vector2[] Nearest(Vector2 v)

        {
            Vector2[] res = new Vector2[0];

            int index = 0;

            Vector2 w = lst[0].vec;

            double min = Vector2.Distance(v, w);

            foreach(DataItem d in lst)

            {
                min = Math.Min(min, Vector2.Distance(v, d.vec));
            }

            foreach (DataItem d in lst)

            {
                if (Vector2.Distance(v,d.vec) == min)
                {
                    Array.Resize(ref res, index + 1);
                    res[index] = d.vec;
                    index++;
                }
            }

            return res;


        }

        public override string ToString()

        {
            return "V3DataCollection " + base.ToString() + ' ' + lst.Count.ToString();
        }

        public override string ToLongString()
        {
            string res = this.ToString();
            foreach (DataItem a in lst)
            {
                res += '\n' + a.ToString();
            }
            return res;
        }
    }

    class V3MainCollection : IEnumerable<V3Data>

    {
        private List<V3Data> lst;

        public int count
        {
            get
            {
                return lst.Count;
            }

        }

        public V3MainCollection()
        {
            List<V3Data> lists = new List<V3Data>();
            lst = lists;
        }

        public IEnumerator<V3Data> GetEnumerator()
        {
            return lst.GetEnumerator();
           
        }

        public void Add(V3Data item)

        {

            lst.Add(item);

        }

        public bool Remove(string id, DateTime w)
        {
            bool res = false;
            foreach (V3Data a in lst)
            {
                if (a.info == id && a.Dt == w)
                {
                    lst.Remove(a);
                    res = true;
                }
            }
            return res;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lst.GetEnumerator();
        }

        public void AddDefaults()
        {
            int count = 6;
            string str = "hn";
            for (int i = 0; i < count; i++)
            {
                float step = i * 0.5f + 0.5f;
                str += "a";
                V3DataOnGrid new1 = new V3DataOnGrid(str, new DateTime(2020, 11, 29), new Grid1D(step, 5), new Grid1D(step, 5));
                V3DataCollection new2 = new V3DataCollection(str, new DateTime(2020, 10, 10));
                new1.InitRandom(0.0f, 10.0f);
                new2.InitRandom(5, step * 5, step * 5, 0.0f, 10.0f);
                lst.Add(new1);
                lst.Add(new2);
            }
        }

        public override string ToString()

        {
            string res = "";

            foreach (V3Data a in lst)

            {

                res += a.ToString();

            }

            return res;
        }
    }


}






class Program

{

    static void Main(string[] args)

    {

        Grid1D temp1 = new Grid1D(0.5f, 5);

        Grid1D temp2 = new Grid1D(0.4f, 4);

        DateTime date1 = new DateTime(2020, 10, 21);

        V3DataOnGrid obj1 = new V3DataOnGrid("1111", date1, temp1, temp2);

        obj1.array = new double[2, 3];

        obj1.InitRandom(4.5, 6.1);

        Console.WriteLine(obj1.ToLongString());

        Console.WriteLine();

        V3DataCollection obj2 = (V3DataCollection)obj1;

        Console.WriteLine(obj2.ToLongString());

        Console.WriteLine();

        V3MainCollection obj3 = new V3MainCollection();

        obj3.AddDefaults();

        Console.WriteLine(obj3.ToString());

        int number = 1;

        Vector2 v = new Vector2(1, 2);

        foreach (V3Data data in obj3)

        {

            Console.WriteLine();

            Console.WriteLine($"Nearest for an element №{number} in V3MainCollection");

            Vector2[] res = data.Nearest(v);

            for (int i = 0; i < res.Length; i++)

            {

                Console.WriteLine(res[i]);

            }

            number++;

        }

        Console.ReadKey();


    }

}
