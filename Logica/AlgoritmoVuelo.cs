// PARTE B: Algoritmo de Vuelo Recursivo con heurística de menor grado (Warnsdorff)
namespace DronSimulator;

public class AlgoritmoVuelo
{
    // Los 8 movimientos posibles en patrón "L" (2x1): 
    // (deltaFila, deltaColumna)
    private static readonly int[] DeltaFila   = { -2, -2,  2,  2, -1, -1,  1,  1 };
    private static readonly int[] DeltaCol    = { -1,  1, -1,  1, -2,  2, -2,  2 };

    private readonly int _n;
    private readonly int[,] _tablero;  // -1 = libre, >= 0 = paso en que fue pisada

    public AlgoritmoVuelo(int n)
    {
        _n = n;
        _tablero = new int[n, n];
        // Inicializar todo como libre
        for (int f = 0; f < n; f++)
            for (int c = 0; c < n; c++)
                _tablero[f, c] = -1;
    }

    // Devuelve la lista de movimientos (fila, col) en orden de visita, 
    // o null si no existe solución.
    public List<(int fila, int col)>? Resolver(int inicioFila, int inicioCol)
    {
        // Contar parcelas alcanzables desde el inicio (BFS/DFS sobre el grafo)
        int alcanzables = ContarAlcanzables(inicioFila, inicioCol);

        var secuencia = new List<(int, int)>();

        _tablero[inicioFila, inicioCol] = 0;
        secuencia.Add((inicioFila, inicioCol));

        if (Backtrack(inicioFila, inicioCol, 1, alcanzables, secuencia))
            return secuencia;

        return null;
    }

    // Cuenta cuántas parcelas son alcanzables desde (fila, col) usando BFS
    private int ContarAlcanzables(int fila, int col)
    {
        var visitado = new bool[_n, _n];
        var cola = new Queue<(int, int)>();
        cola.Enqueue((fila, col));
        visitado[fila, col] = true;
        int count = 0;

        while (cola.Count > 0)
        {
            var (f, c) = cola.Dequeue();
            count++;
            for (int k = 0; k < 8; k++)
            {
                int nf = f + DeltaFila[k];
                int nc = c + DeltaCol[k];
                if (EsValida(nf, nc) && !visitado[nf, nc])
                {
                    visitado[nf, nc] = true;
                    cola.Enqueue((nf, nc));
                }
            }
        }
        return count;
    }

    // Recursión con backtracking y heurística de menor grado
    private bool Backtrack(int fila, int col, int paso, int objetivo, List<(int, int)> secuencia)
    {
        if (paso == objetivo)
            return true;

        // Obtener candidatos válidos (libres y dentro del terreno)
        var candidatos = ObtenerCandidatos(fila, col);

        // Ordenar por grado ascendente (heurística de Warnsdorff)
        candidatos.Sort((a, b) => Grado(a.f, a.c).CompareTo(Grado(b.f, b.c)));

        foreach (var (nf, nc) in candidatos)
        {
            _tablero[nf, nc] = paso;
            secuencia.Add((nf, nc));

            if (Backtrack(nf, nc, paso + 1, objetivo, secuencia))
                return true;

            // Deshacer (backtrack)
            _tablero[nf, nc] = -1;
            secuencia.RemoveAt(secuencia.Count - 1);
        }

        return false;
    }

    // Devuelve los destinos válidos (dentro del terreno y libres) desde (fila, col)
    private List<(int f, int c)> ObtenerCandidatos(int fila, int col)
    {
        var lista = new List<(int, int)>();
        for (int k = 0; k < 8; k++)
        {
            int nf = fila + DeltaFila[k];
            int nc = col  + DeltaCol[k];
            if (EsValida(nf, nc) && _tablero[nf, nc] == -1)
                lista.Add((nf, nc));
        }
        return lista;
    }

    // Calcula el grado de una parcela: cuántas salidas libres tiene
    private int Grado(int fila, int col)
    {
        int count = 0;
        for (int k = 0; k < 8; k++)
        {
            int nf = fila + DeltaFila[k];
            int nc = col  + DeltaCol[k];
            if (EsValida(nf, nc) && _tablero[nf, nc] == -1)
                count++;
        }
        return count;
    }

    private bool EsValida(int fila, int col) =>
        fila >= 0 && fila < _n && col >= 0 && col < _n;

    // Devuelve el tablero para mostrarlo (copia de solo lectura)
    public int[,] ObtenerTablero() => _tablero;
}
