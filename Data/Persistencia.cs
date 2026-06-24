// PARTE D: Persistencia con ADO.NET síncrono, transacción y ofuscación
using Npgsql;

namespace DronSimulator;

public class Persistencia
{
    private readonly string _connectionString;

    public Persistencia(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Guarda la cabecera + detalle en una transacción atómica.
    // Devuelve el ID generado en tb_master_control.
    public int GuardarSimulacion(int n, int inicioX, int inicioY, List<(int fila, int col)> secuencia)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var trans = conn.BeginTransaction();
        try
        {
            // --- 1. Insertar cabecera y recuperar ID con RETURNING ---
            int masterId;
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO tb_master_control (fecha, terreno_n, coord_x, coord_y) " +
                "VALUES (NOW(), @n, @x, @y) RETURNING id", conn, trans))
            {
                cmd.Parameters.AddWithValue("@n", n);
                cmd.Parameters.AddWithValue("@x", inicioX);
                cmd.Parameters.AddWithValue("@y", inicioY);
                masterId = (int)cmd.ExecuteScalar()!;
            }

            // --- 2. Insertar movimientos con while + índice manual (sin for/foreach) ---
            int i = 0;
            while (i < secuencia.Count)
            {
                int pasoReal = i;  // el índice en la secuencia ES el número de paso

                // REGLA DE OFUSCACIÓN:
                // Par  -> guardar pasoReal * 2
                // Impar -> guardar -pasoReal (negativo)
                int pasoOfuscado = (pasoReal % 2 == 0)
                    ? pasoReal * 2
                    : -pasoReal;

                using var cmdDet = new NpgsqlCommand(
                    "INSERT INTO tb_det_log (master_id, paso_ofuscado, pos_x, pos_y) " +
                    "VALUES (@mid, @paso, @x, @y)", conn, trans);

                cmdDet.Parameters.AddWithValue("@mid",  masterId);
                cmdDet.Parameters.AddWithValue("@paso", pasoOfuscado);
                cmdDet.Parameters.AddWithValue("@x",    secuencia[i].fila);
                cmdDet.Parameters.AddWithValue("@y",    secuencia[i].col);
                cmdDet.ExecuteNonQuery();

                i++;
            }

            trans.Commit();
            return masterId;
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    // PARTE E: Lee los últimos 5 registros del detalle y aplica ingeniería inversa
    public void MostrarUltimos5(int masterId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "SELECT paso_ofuscado, pos_x, pos_y " +
            "FROM tb_det_log " +
            "WHERE master_id = @mid " +
            "ORDER BY id DESC LIMIT 5", conn);

        cmd.Parameters.AddWithValue("@mid", masterId);

        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n--- Últimos 5 pasos del recorrido (reconstruidos) ---");
        Console.WriteLine($"{"Paso Real",-12} {"Fila",-8} {"Col",-8}");
        Console.WriteLine(new string('-', 30));

        while (reader.Read())
        {
            int pasoOfuscado = reader.GetInt32(0);
            int posX         = reader.GetInt32(1);
            int posY         = reader.GetInt32(2);

            // INGENIERÍA INVERSA de la ofuscación:
            // Si guardado < 0  -> era impar, paso real = -guardado
            // Si guardado >= 0 -> era par,   paso real = guardado / 2
            int pasoReal = (pasoOfuscado < 0)
                ? -pasoOfuscado
                : pasoOfuscado / 2;

            Console.WriteLine($"{pasoReal,-12} {posX,-8} {posY,-8}");
        }
    }
}
