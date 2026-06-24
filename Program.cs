// PARTE C + E: Program.cs - Punto de entrada, configuración y flujo principal
using Microsoft.Extensions.Configuration;
using DronSimulator;

// ============================================================
// PARTE C: Cargar configuración desde appsettings.json
// ============================================================
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

string connectionString = config.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión en appsettings.json");

// ============================================================
// PARTE E: Validación de entradas
// ============================================================
int n = LeerEnteroPositivo("Ingrese el tamaño del terreno N (entero >= 1): ");

int inicioX = LeerCoordenada($"Ingrese la coordenada X de despegue (fila, 0 a {n - 1}): ", n);
int inicioY = LeerCoordenada($"Ingrese la coordenada Y de despegue (columna, 0 a {n - 1}): ", n);

Console.WriteLine($"\nTerreno: {n}x{n} | Despegue: ({inicioX}, {inicioY})\n");

// ============================================================
// PARTE B: Ejecutar el algoritmo de vuelo
// ============================================================
var algoritmo = new AlgoritmoVuelo(n);
var secuencia = algoritmo.Resolver(inicioX, inicioY);

// ============================================================
// PARTE E: Mostrar matriz en consola
// ============================================================
int[,] tablero = algoritmo.ObtenerTablero();
MostrarMatriz(tablero, n);

if (secuencia == null)
{
    Console.WriteLine("\nResultado: SIN SOLUCIÓN — el dron no pudo cubrir todas las parcelas alcanzables.");
    Console.WriteLine("(Las parcelas pueden ser alcanzables pero no existe ruta sin repetir.)");
}
else
{
    Console.WriteLine($"\nResultado: ÉXITO — el dron cubrió {secuencia.Count} parcela(s) en {secuencia.Count - 1} salto(s).");

    // ============================================================
    // PARTE D: Persistir en PostgreSQL
    // ============================================================
    try
    {
        var db = new Persistencia(connectionString);
        int masterId = db.GuardarSimulacion(n, inicioX, inicioY, secuencia);
        Console.WriteLine($"Simulación guardada en base de datos. ID generado: {masterId}");

        // PARTE E: Reporte inverso
        db.MostrarUltimos5(masterId);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError al guardar en base de datos: {ex.Message}");
    }
}

// ============================================================
// FUNCIONES AUXILIARES
// ============================================================

static int LeerEnteroPositivo(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        string? entrada = Console.ReadLine();
        if (int.TryParse(entrada, out int valor) && valor >= 1)
            return valor;
        Console.WriteLine("  [!] Valor inválido. Debe ser un entero >= 1.");
    }
}

static int LeerCoordenada(string mensaje, int n)
{
    while (true)
    {
        Console.Write(mensaje);
        string? entrada = Console.ReadLine();
        if (int.TryParse(entrada, out int valor) && valor >= 0 && valor < n)
            return valor;
        Console.WriteLine($"  [!] Valor inválido. Debe estar en el rango [0, {n - 1}].");
    }
}

static void MostrarMatriz(int[,] tablero, int n)
{
    Console.WriteLine("Matriz del recorrido (0 = despegue, '.' = no alcanzable):\n");
    for (int f = 0; f < n; f++)
    {
        for (int c = 0; c < n; c++)
        {
            if (tablero[f, c] == -1)
                Console.Write($"{".",5}");
            else
                Console.Write($"{tablero[f, c],5}");
        }
        Console.WriteLine();
    }
}
