using System;

using System.Collections.Generic;

using System.Numerics;

using System.Collections;

using System.Linq;

using System.IO;
using lab_2_var_3_1;

namespace lab_2_var_3_1
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

        public string Tostring(string format)

        {
            return $"elec={elec.ToString(format)}; vec.ToString(format)";

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

        public string ToString(string format)

        {
            return $"step={step.ToString(format)}; count={count}";

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

        public abstract string ToLongString(string format);

        public override string ToString()

        {

            return $"info={info}; dt={Dt}";

        }

    }

    class V3DataOnGrid : V3Data, IEnumerable<DataItem>

    {
        class V3DataOnGridEnumerator : IEnumerator<DataItem>
        {
            DataItem[,] values;
            int position1 = 0;
            int position2 = -1;
            public V3DataOnGridEnumerator(double[,] values_, Grid1D grid_x, Grid1D grid_y)
            {
                values = new DataItem[grid_x.count, grid_y.count];
                for (int i = 0; i < grid_x.count; i++)
                {
                    for (int j = 0; j < grid_y.count; j++)
                    {
                        values[i, j] = new DataItem(new Vector2(i * grid_x.step, j * grid_y.step), values_[i, j]);
                    }
                }
            }

            void IDisposable.Dispose() { }

            public bool MoveNext()
            {
                if (position2 < values.GetLength(1) - 1)
                {
                    position2++;
                    return true;
                }
                else if (position2 == values.GetLength(1) - 1 && position1 < values.GetLength(0) - 1)
                {
                    position1++;
                    position2 = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public DataItem Current
            {
                get
                {
                    if (position1 >= 0 && position2 >= 0 && position1 <= values.GetLength(0) && position2 <= values.GetLength(1))
                        return values[position1, position2];
                    {
                    }
                    throw new InvalidOperationException();
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            public void Reset()
            {
                position1 = 0;
                position2 = -1;
            }
        }
        public Grid1D grid_x { set; get; }

        public Grid1D grid_y { set; get; }

        public double[,] array { set; get; }

        public V3DataOnGrid(string id, DateTime w, Grid1D grid_x, Grid1D grid_y) : base(id, w)

        {
            Dt = w;

            info = id;

            array = new double[grid_x.count, grid_y.count];

        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            return new V3DataOnGridEnumerator(array, grid_x, grid_y);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

                    array[i, j] = x;

                }

            }

        }

        public override System.Numerics.Vector2[] Nearest(Vector2 v)

        {

            Vector2[] res = new Vector2[0];

            int index = 0;

            Vector2 w = new System.Numerics.Vector2(0, 0);

            double min = Vector2.Distance(v, w);

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

        public V3DataOnGrid(string filename) : base("", new DateTime())
        {
            // .: FORMAT :.
            // {string   info}\n
            // {DateTime Dt}\n
            // {float StepX}\n
            // {int NStepsX}\n
            // {float StepY}\n
            // {int NStepsY}\n
            // {double array[0, 0]}\n
            // {double array[0, 1]}\n
            // ...
            // {double array[0, NStepsY]}\n
            // {double array[1, 0]}\n
            // ...
            // {double array[NStepsX, NStepsY]}\n

            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                // base class
                info = sr.ReadLine();
                Dt = Convert.ToDateTime(sr.ReadLine());

                float Step;
                int NSteps;
                // grid_x
                Step = (float)Convert.ToDouble(sr.ReadLine());
                NSteps = Convert.ToInt32(sr.ReadLine());
                grid_x = new Grid1D(Step, NSteps);
                // grid_y
                Step = (float)Convert.ToDouble(sr.ReadLine());
                NSteps = Convert.ToInt32(sr.ReadLine());
                grid_y = new Grid1D(Step, NSteps);

                // array[,]
                array = new double[grid_x.count, grid_y.count];
                for (int i = 0; i < grid_x.count; ++i)
                {
                    for (int j = 0; j < grid_y.count; ++j)
                    {
                        array[i, j] = Convert.ToDouble(sr.ReadLine());
                    }
                }

                sr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fs != null) { fs.Close(); }
            }
        }

        public override string ToLongString(string format)
        {
            string res = this.ToString() + "\n";

            for (int i = 0; i < grid_x.count; ++i)
            {
                for (int j = 0; j < grid_y.count; ++j)
                {
                    res += $"({(i * grid_x.step).ToString(format)}, {(j * grid_y.step).ToString(format)}) " +
                           $": {array[i, j].ToString(format)}\n";
                }
            }

            return res;
        }

    }

    class V3DataCollection : V3Data, IEnumerable<DataItem>
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

                lst.Add(new DataItem(new System.Numerics.Vector2(x, y), z));
            }
        }

        public override Vector2[] Nearest(Vector2 v)

        {
            Vector2[] res = new Vector2[0];

            int index = 0;

            Vector2 w = lst[0].vec;

            double min = Vector2.Distance(v, w);

            foreach (DataItem d in lst)

            {
                min = Math.Min(min, Vector2.Distance(v, d.vec));
            }

            foreach (DataItem d in lst)

            {
                if (Vector2.Distance(v, d.vec) == min)
                {
                    Array.Resize(ref res, index + 1);
                    res[index] = d.vec;
                    index++;
                }
            }

            return res;


        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            return lst.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public override string ToLongString(string format)
        {
            string res = this.ToString() + "\n";

            foreach (DataItem elem in lst)
            {
                res += $"({elem.vec.X.ToString(format)}, {elem.vec.Y.ToString(format)}) : {elem.elec.ToString(format)}\n";
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

        IEnumerator IEnumerable.GetEnumerator()
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

        public void AddDefaults()
        {
            int count = 6;
            string str = "hn";
            for (int i = 0; i < count; i++)
            {
                float step = i * 0.5f + 0.5f;
                str += "a";
                V3DataOnGrid new1 = new V3DataOnGrid(str, new DateTime(2020, 11, 29), new Grid1D(step, 5), new Grid1D(step, 5));
                V3DataCollection new2 = new V3DataCollection(str, new DateTime(2020, 11, 29));
                new1.InitRandom(0.0f, 10.0f);
                new2.InitRandom(5, step * 5, step * 5, 0.0f, 10.0f);
                lst.Add(new1);
                lst.Add(new2);
            }
        }

        // Минимальное расстояние между v и точками из Data, в которых измерено поле
        public float RMin(Vector2 v)
        {
            return lst
                  .Select(DataCollectionCast)
                  .Where(x => x.Count() > 0)
                  .Select(x => x.Nearest(v))
                  .Select(x => Vector2.Distance(x.First(), v))
                  .Min();
        }

        // Результат всех измерений, который находится ближе всех к v
        public DataItem RMinDataItem(Vector2 v)
        {
            var queryToDataCollection = lst.Select(DataCollectionCast);
            var queryToDataItem = from data in queryToDataCollection
                                  from vector in data
                                  select vector;

            return queryToDataItem.OrderBy(x => Vector2.Distance(x.vec, v)).First();
        }

        // Перечисляет все точки измерения поля, такие, что они есть в элементах типа V3DataCollection,
        // но их нет в элементах типа V3DataOnGrid
        public IEnumerable<Vector2> DataCollectionExceptDataOnGrid
        {
            get
            {
                var dataOnGrid = lst.Where(x => x is V3DataOnGrid)
                                         .Select(DataCollectionCast);
                var dataCollection = lst.Where(x => x is V3DataCollection)
                                         .Select(DataCollectionCast);

                var vectorsDataOnGrid = from lst in dataOnGrid
                                        from vector in lst
                                        select vector.vec;

                var vectorsDataCollection = from data in dataCollection
                                            from vector in data
                                            select vector.vec;

                return vectorsDataCollection.Except(vectorsDataOnGrid).Distinct();
            }
        }

        private V3DataCollection DataCollectionCast(V3Data elem)
        {
            return elem is V3DataOnGrid ? (V3DataCollection)(elem as V3DataOnGrid) : elem as V3DataCollection;
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

        public string ToLongString(string format)
        {
            string result = "";

            foreach (V3Data elem in lst)
            {
                result += elem.ToLongString(format) + '\n';
            }

            return result;
        }
    }


}





class Program
{
    static void Main(string[] args)
    {
        // 1)
        V3DataOnGrid task1 = new V3DataOnGrid("/Users/homidov/Desktop/data.txt");
        Console.WriteLine(task1.ToLongString("F3"));

        // 2)
        V3MainCollection task2 = new V3MainCollection();
        task2.AddDefaults();

        // 3)
        // RMin
        Console.WriteLine($"RMin(3, 4):\n{task2.RMin(new Vector2(3, 4))}\n");

        // RMinDataItem
        Console.WriteLine($"RMinDataItem(3, 4):\n{task2.RMinDataItem(new Vector2(3, 4))}\n");

        // DataCollectionExceptDataOnGrid
        Console.WriteLine("DataCollectionExceptDataOnGrid:");
        var task3 = task2.DataCollectionExceptDataOnGrid;
        foreach (Vector2 vec in task3)
        {
            Console.WriteLine(vec);
        }
    }
}
