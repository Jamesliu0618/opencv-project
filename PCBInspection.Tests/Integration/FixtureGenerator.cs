using OpenCvSharp;

namespace PCBInspection.Tests.Integration
{
	public static class FixtureGenerator
	{
		// Generate a chessboard image for calibration
		public static void CreateChessboard(string path, int patternCols, int patternRows, int squarePx)
		{
			// patternCols/patternRows are inner corners; squares are patternCols+1 x patternRows+1
			int cols   = patternCols + 1;
			int rows   = patternRows + 1;
			int width  = cols * squarePx;
			int height = rows * squarePx;

			using(var img = new Mat(new Size(width, height), MatType.CV_8UC1, Scalar.All(255)))
			{
				for(int y = 0; y < rows; y++)
				{
					for(int x = 0; x < cols; x++)
					{
						bool black = (x + y) % 2 == 0;
						var  rect  = new Rect(x * squarePx, y * squarePx, squarePx, squarePx);
						Cv2.Rectangle(img, rect, black ? Scalar.Black : Scalar.White, -1);
					}
				}
				Cv2.ImWrite(path, img);
			}
		}

		// Generate a synthetic PCB image with two rectangular components at known pixels
		public static void CreatePcbWithTwoComponents(string path, int width = 640, int height = 480, int c1CenterX = 100, int c1CenterY = 120, int c1W = 40, int c1H = 20, int c2CenterX = 220, int c2CenterY = 120, int c2W = 40, int c2H = 20)
		{
			using(var img = new Mat(new Size(width, height), MatType.CV_8UC3, Scalar.White))
			{
				var rect1 = new Rect(c1CenterX - c1W / 2, c1CenterY - c1H / 2, c1W, c1H);
				var rect2 = new Rect(c2CenterX - c2W / 2, c2CenterY - c2H / 2, c2W, c2H);
				Cv2.Rectangle(img, rect1, Scalar.Black, -1);
				Cv2.Rectangle(img, rect2, Scalar.Black, -1);
				Cv2.ImWrite(path, img);
			}
		}
	}
}