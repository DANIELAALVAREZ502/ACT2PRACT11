using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Globalization;

namespace ACT2DemoIntroAsync_6439050
{
    public partial class Form1 : Form
    {
        HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();

            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "ACT2DemoIntroAsync/1.0 (https://es.wikipedia.org/wiki/Templo_tazumal; estudiante@correo.com)");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            button1.Enabled = false;
            textBox1.Clear();

            try
            {
                var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
                var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado.secuencial");
                var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado.paralelo");
                PrepararEjecucion(destinoBaseParalelo, destinoBaseSecuencial);

                Mostrar("Inicio");
                List<Imagen> imagenes = ObtenerImagenes();

                //Parte secuencial 

                var sw = new Stopwatch();

                sw.Start();

                foreach (var imagen in imagenes)
                {
                    await ProcesarImagen(destinoBaseSecuencial, imagen);
                }

                var segundosSecuencial = sw.ElapsedMilliseconds / 1000.0;
                Mostrar($"Secuencial - duración en segundos: {segundosSecuencial.ToString("0.000", CultureInfo.InvariantCulture)}");

                sw.Reset();

                sw.Start();

                var tareasEnumerable = imagenes.Select(async imagen =>
                {
                  await ProcesarImagen(destinoBaseParalelo, imagen);
                });

                await Task.WhenAll(tareasEnumerable);

                var segundosParalelo = sw.ElapsedMilliseconds / 1000.0;
                Mostrar($"Paralelo - duración en segundos: {segundosParalelo.ToString("0.000", CultureInfo.InvariantCulture)}");

                sw.Stop();
            }
            catch (Exception ex)
            {
                Mostrar($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                pictureBox1.Visible = false;
                button1.Enabled = true;
            }
        }

        private void Mostrar(string mensaje)
        {
            Console.WriteLine(mensaje);
            textBox1.AppendText(mensaje + Environment.NewLine);
        }

        private async Task<byte[]> DescargarBytes(string url)
        {
            for (int intento = 1; ; intento++)
            {
                using var respuesta = await httpClient.GetAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    return await respuesta.Content.ReadAsByteArrayAsync();
                }

                if (intento >= 3)
                {
                    throw new HttpRequestException($"Error HTTP {(int)respuesta.StatusCode} al descargar: {url}");
                }

                await Task.Delay(1000 * intento);
            }
        }

        private async Task ProcesarImagen(string directorio,Imagen imagen)
        {
            var contenido = await DescargarBytes(imagen.URL);

            using var ms = new MemoryStream(contenido);
            using var bitmap = new Bitmap(ms);

            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var destino = Path.Combine(directorio, imagen.Nombre);
            bitmap.Save(destino);
        }

        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();

            for (int i = 0; i < 7; i++)
            {
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Templo tazumal {i}.jpg",
                        URL = "https://thumb.wikimedia.org/wikipedia/commons/thumb/d/df/Templo_tazumal.jpg/1280px-Templo_tazumal.jpg?utm_source=es.wikipedia.org&utm_campaign=imageinfo&utm_content=thumbnail"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Indigofera tinctoria0 {i}.jpg",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/8/83/Indigofera_tinctoria0.jpg?utm_source=es.wikipedia.org&utm_campaign=imageinfo&utm_content=thumbnail_unscaled"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Mapa geomorfológico simplificado de El Salvador {i}.png",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/9/90/Mapa_geomorfol%C3%B3gico_simplificado_de_El_Salvador.png?utm_source=es.wikipedia.org&utm_campaign=imageinfo&utm_content=thumbnail_unscaled"
                    });
            }
            return imagenes;
        }

        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);
            foreach(var archivo in archivos)
            {
                File.Delete(archivo);
            }
        }

        private void PrepararEjecucion(string destinoBaseParalelo,string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }

            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseSecuencial);
            BorrarArchivos(destinoBaseParalelo);
        }


        private async Task<string> ProcesamientoLargo()
        {
            await Task.Delay(1000); //asincrona

            return "Felipe";
        }
        
        private async Task RealizarProcesamientoA()
        {
            await Task.Delay(1000); //asincrona
            Console.WriteLine("Procesamiento A terminado");
        }

        private async Task RealizarProcesamientoB()
        {
            await Task.Delay(1000); //asincrona
            Console.WriteLine("Procesamiento B terminado");
        }

        private async Task RealizarProcesamientoLargoC()
        {
            await Task.Delay(1000); //asincrona
            Console.WriteLine("Procesamiento C terminado");
        }

    }
}
