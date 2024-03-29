namespace pragmatic_quant_model.Maths.Sobol
{
	internal static class StdDirections
	{
		#region Sobol Levitan
		/* Sobol' Levitan coefficients of the free direction integers as given by Bratley, P., Fox, B.L. (1988) */
		public static uint[][] SLinitializers()
		{
		    uint[][] result =
		    {
		        new uint[] {1, 0},
		        new uint[] {1, 1, 0},
		        new uint[] {1, 3, 7, 0},
		        new uint[] {1, 1, 5, 0},
		        new uint[] {1, 3, 1, 1, 0},
		        new uint[] {1, 1, 3, 7, 0},
		        new uint[] {1, 3, 3, 9, 9, 0},
		        new uint[] {1, 3, 7, 13, 3, 0},
		        new uint[] {1, 1, 5, 11, 27, 0},
		        new uint[] {1, 3, 5, 1, 15, 0},
		        new uint[] {1, 1, 7, 3, 29, 0},
		        new uint[] {1, 3, 7, 7, 21, 0},
		        new uint[] {1, 1, 1, 9, 23, 37, 0},
		        new uint[] {1, 3, 3, 5, 19, 33, 0},
		        new uint[] {1, 1, 3, 13, 11, 7, 0},
		        new uint[] {1, 1, 7, 13, 25, 5, 0},
		        new uint[] {1, 3, 5, 11, 7, 11, 0},
		        new uint[] {1, 1, 1, 3, 13, 39, 0},
		        new uint[] {1, 3, 1, 15, 17, 63, 13, 0},
		        new uint[] {1, 1, 5, 5, 1, 27, 33, 0},
		        new uint[] {1, 3, 3, 3, 25, 17, 115, 0},
		        new uint[] {1, 1, 3, 15, 29, 15, 41, 0},
		        new uint[] {1, 3, 1, 7, 3, 23, 79, 0},
		        new uint[] {1, 3, 7, 9, 31, 29, 17, 0},
		        new uint[] {1, 1, 5, 13, 11, 3, 29, 0},
		        new uint[] {1, 3, 1, 9, 5, 21, 119, 0},
		        new uint[] {1, 1, 3, 1, 23, 13, 75, 0},
		        new uint[] {1, 3, 3, 11, 27, 31, 73, 0},
		        new uint[] {1, 1, 7, 7, 19, 25, 105, 0},
		        new uint[] {1, 3, 5, 5, 21, 9, 7, 0},
		        new uint[] {1, 1, 1, 15, 5, 49, 59, 0},
		        new uint[] {1, 1, 1, 1, 1, 33, 65, 0},
		        new uint[] {1, 3, 5, 15, 17, 19, 21, 0},
		        new uint[] {1, 1, 7, 11, 13, 29, 3, 0},
		        new uint[] {1, 3, 7, 5, 7, 11, 113, 0},
		        new uint[] {1, 1, 5, 3, 15, 19, 61, 0},
		        new uint[] {1, 3, 1, 1, 9, 27, 89, 7, 0},
		        new uint[] {1, 1, 3, 7, 31, 15, 45, 23, 0},
		        new uint[] {1, 3, 3, 9, 9, 25, 107, 39, 0}
		    };
			return result;		
		}
		#endregion
		
		#region Jaeckel
		/* coefficients of the free direction integers as given in
           "Monte Carlo Methods in Finance", by Peter Jдckel, section 8.3
        */
		public static uint[][] JaeckelInitializers()
		{
		    uint[][] result =
		    {
                //Sobol first vectors
                new uint[] { 1, 0 },
                new uint[] { 1, 1, 0 },
                new uint[] { 1, 3, 7, 0 },
                new uint[] { 1, 1, 5, 0 },
                new uint[] { 1, 3, 1, 1, 0 },
                new uint[] { 1, 1, 3, 7, 0 },
                new uint[] { 1, 3, 3, 9, 9, 0 },
                //
		        new uint[] {1, 3, 7, 7, 21, 0},
		        new uint[] {1, 1, 5, 11, 27, 0},
		        new uint[] {1, 1, 7, 3, 29, 0},
		        new uint[] {1, 3, 7, 13, 3, 0},
		        new uint[] {1, 3, 5, 1, 15, 0},
		        new uint[] {1, 1, 1, 9, 23, 37, 0},
		        new uint[] {1, 1, 3, 13, 11, 7, 0},
		        new uint[] {1, 3, 3, 5, 19, 33, 0},
		        new uint[] {1, 1, 7, 13, 25, 5, 0},
		        new uint[] {1, 1, 1, 3, 13, 39, 0},
		        new uint[] {1, 3, 5, 11, 7, 11, 0},
		        new uint[] {1, 3, 1, 7, 3, 23, 79, 0},
		        new uint[] {1, 3, 1, 15, 17, 63, 13, 0},
		        new uint[] {1, 3, 3, 3, 25, 17, 115, 0},
		        new uint[] {1, 3, 7, 9, 31, 29, 17, 0},
		        new uint[] {1, 1, 3, 15, 29, 15, 41, 0},
		        new uint[] {1, 3, 1, 9, 5, 21, 119, 0},
		        new uint[] {1, 1, 5, 5, 1, 27, 33, 0},
		        new uint[] {1, 1, 3, 1, 23, 13, 75, 0},
		        new uint[] {1, 1, 7, 7, 19, 25, 105, 0},
		        new uint[] {1, 3, 5, 5, 21, 9, 7, 0},
		        new uint[] {1, 1, 1, 15, 5, 49, 59, 0},
		        new uint[] {1, 3, 5, 15, 17, 19, 21, 0},
		        new uint[] {1, 1, 7, 11, 13, 29, 3, 0}
		    };
			return result;
		}		
		#endregion
	
		#region Lemieux
		/* Lemieux coefficients of the free direction integers as given
           in Christiane Lemieux, private communication, September 2004
        */
		public static uint[][] LemieuxInitializers()
		{
		    uint[][] result =
		    {
                //Sobol vectors
                new uint[] {1, 0},
		        new uint[] {1, 1, 0},
		        new uint[] {1, 3, 7, 0},
		        new uint[] {1, 1, 5, 0},
		        new uint[] {1, 3, 1, 1, 0},
		        new uint[] {1, 1, 3, 7, 0},
		        new uint[] {1, 3, 3, 9, 9, 0},
		        new uint[] {1, 3, 7, 13, 3, 0},
		        new uint[] {1, 1, 5, 11, 27, 0},
		        new uint[] {1, 3, 5, 1, 15, 0},
		        new uint[] {1, 1, 7, 3, 29, 0},
		        new uint[] {1, 3, 7, 7, 21, 0},
		        new uint[] {1, 1, 1, 9, 23, 37, 0},
		        new uint[] {1, 3, 3, 5, 19, 33, 0},
		        new uint[] {1, 1, 3, 13, 11, 7, 0},
		        new uint[] {1, 1, 7, 13, 25, 5, 0},
		        new uint[] {1, 3, 5, 11, 7, 11, 0},
		        new uint[] {1, 1, 1, 3, 13, 39, 0},
		        new uint[] {1, 3, 1, 15, 17, 63, 13, 0},
		        new uint[] {1, 1, 5, 5, 1, 27, 33, 0},
		        new uint[] {1, 3, 3, 3, 25, 17, 115, 0},
		        new uint[] {1, 1, 3, 15, 29, 15, 41, 0},
		        new uint[] {1, 3, 1, 7, 3, 23, 79, 0},
		        new uint[] {1, 3, 7, 9, 31, 29, 17, 0},
		        new uint[] {1, 1, 5, 13, 11, 3, 29, 0},
		        new uint[] {1, 3, 1, 9, 5, 21, 119, 0},
		        new uint[] {1, 1, 3, 1, 23, 13, 75, 0},
		        new uint[] {1, 3, 3, 11, 27, 31, 73, 0},
		        new uint[] {1, 1, 7, 7, 19, 25, 105, 0},
		        new uint[] {1, 3, 5, 5, 21, 9, 7, 0},
		        new uint[] {1, 1, 1, 15, 5, 49, 59, 0},
		        new uint[] {1, 1, 1, 1, 1, 33, 65, 0},
		        new uint[] {1, 3, 5, 15, 17, 19, 21, 0},
		        new uint[] {1, 1, 7, 11, 13, 29, 3, 0},
		        new uint[] {1, 3, 7, 5, 7, 11, 113, 0},
		        new uint[] {1, 1, 5, 3, 15, 19, 61, 0},
		        new uint[] {1, 3, 1, 1, 9, 27, 89, 7, 0},
		        new uint[] {1, 1, 3, 7, 31, 15, 45, 23, 0},
		        new uint[] {1, 3, 3, 9, 9, 25, 107, 39, 0},
                //
		        new uint[] {1, 1, 3, 13, 7, 35, 61, 91, 0},
		        new uint[] {1, 1, 7, 11, 5, 35, 55, 75, 0},
		        new uint[] {1, 3, 5, 5, 11, 23, 29, 139, 0},
		        new uint[] {1, 1, 1, 7, 11, 15, 17, 81, 0},
		        new uint[] {1, 1, 7, 9, 5, 57, 79, 103, 0},
		        new uint[] {1, 1, 7, 13, 19, 5, 5, 185, 0},
		        new uint[] {1, 3, 1, 3, 13, 57, 97, 131, 0},
		        new uint[] {1, 1, 5, 5, 21, 25, 125, 197, 0},
		        new uint[] {1, 3, 3, 9, 31, 11, 103, 201, 0},
		        new uint[] {1, 1, 5, 3, 7, 25, 51, 121, 0},
		        new uint[] {1, 3, 7, 15, 19, 53, 73, 189, 0},
		        new uint[] {1, 1, 1, 15, 19, 55, 27, 183, 0},
		        new uint[] {1, 1, 7, 13, 3, 29, 109, 69, 0},
		        new uint[] {1, 1, 5, 15, 15, 23, 15, 1, 57, 0},
		        new uint[] {1, 3, 1, 3, 23, 55, 43, 143, 397, 0},
		        new uint[] {1, 1, 3, 11, 29, 9, 35, 131, 411, 0},
		        new uint[] {1, 3, 1, 7, 27, 39, 103, 199, 277, 0},
		        new uint[] {1, 3, 7, 3, 19, 55, 127, 67, 449, 0},
		        new uint[] {1, 3, 7, 3, 5, 29, 45, 85, 3, 0},
		        new uint[] {1, 3, 5, 5, 13, 23, 75, 245, 453, 0},
		        new uint[] {1, 3, 1, 15, 21, 47, 3, 77, 165, 0},
		        new uint[] {1, 1, 7, 9, 15, 5, 117, 73, 473, 0},
		        new uint[] {1, 3, 1, 9, 1, 21, 13, 173, 313, 0},
		        new uint[] {1, 1, 7, 3, 11, 45, 63, 77, 49, 0},
		        new uint[] {1, 1, 1, 1, 1, 25, 123, 39, 259, 0},
		        new uint[] {1, 1, 1, 5, 23, 11, 59, 11, 203, 0},
		        new uint[] {1, 3, 3, 15, 21, 1, 73, 71, 421, 0},
		        new uint[] {1, 1, 5, 11, 15, 31, 115, 95, 217, 0},
		        new uint[] {1, 1, 3, 3, 7, 53, 37, 43, 439, 0},
		        new uint[] {1, 1, 1, 1, 27, 53, 69, 159, 321, 0},
		        new uint[] {1, 1, 5, 15, 29, 17, 19, 43, 449, 0},
		        new uint[] {1, 1, 3, 9, 1, 55, 121, 205, 255, 0},
		        new uint[] {1, 1, 3, 11, 9, 47, 107, 11, 417, 0},
		        new uint[] {1, 1, 1, 5, 17, 25, 21, 83, 95, 0},
		        new uint[] {1, 3, 5, 13, 31, 25, 61, 157, 407, 0},
		        new uint[] {1, 1, 7, 9, 25, 33, 41, 35, 17, 0},
		        new uint[] {1, 3, 7, 15, 13, 39, 61, 187, 461, 0},
		        new uint[] {1, 3, 7, 13, 5, 57, 23, 177, 435, 0},
		        new uint[] {1, 1, 3, 15, 11, 27, 115, 5, 337, 0},
		        new uint[] {1, 3, 7, 3, 15, 63, 61, 171, 339, 0},
		        new uint[] {1, 3, 3, 13, 15, 61, 59, 47, 1, 0},
		        new uint[] {1, 1, 5, 15, 13, 5, 39, 83, 329, 0},
		        new uint[] {1, 1, 5, 5, 5, 27, 25, 39, 301, 0},
		        new uint[] {1, 1, 5, 11, 31, 41, 35, 233, 27, 0},
		        new uint[] {1, 3, 5, 15, 7, 37, 119, 171, 419, 0},
		        new uint[] {1, 3, 5, 5, 3, 29, 21, 189, 417, 0},
		        new uint[] {1, 1, 1, 1, 21, 41, 117, 119, 351, 0},
		        new uint[] {1, 1, 3, 1, 7, 27, 87, 19, 213, 0},
		        new uint[] {1, 1, 1, 1, 17, 7, 97, 217, 477, 0},
		        new uint[] {1, 1, 7, 1, 29, 61, 103, 231, 269, 0},
		        new uint[] {1, 1, 7, 13, 9, 27, 107, 207, 311, 0},
		        new uint[] {1, 1, 7, 5, 25, 21, 107, 179, 423, 0},
		        new uint[] {1, 3, 5, 11, 7, 1, 17, 245, 281, 0},
		        new uint[] {1, 3, 5, 9, 1, 5, 53, 59, 125, 0},
		        new uint[] {1, 1, 7, 1, 31, 57, 71, 245, 125, 0},
		        new uint[] {1, 1, 7, 5, 5, 57, 53, 253, 441, 0},
		        new uint[] {1, 3, 1, 13, 19, 35, 119, 235, 381, 0},
		        new uint[] {1, 3, 1, 7, 19, 59, 115, 33, 361, 0},
		        new uint[] {1, 1, 3, 5, 13, 1, 49, 143, 501, 0},
		        new uint[] {1, 1, 3, 5, 1, 63, 101, 85, 189, 0},
		        new uint[] {1, 1, 5, 11, 27, 63, 13, 131, 5, 0},
		        new uint[] {1, 1, 5, 7, 15, 45, 75, 59, 455, 585, 0},
		        new uint[] {1, 3, 1, 3, 7, 7, 111, 23, 119, 959, 0},
		        new uint[] {1, 3, 3, 9, 11, 41, 109, 163, 161, 879, 0},
		        new uint[] {1, 3, 5, 1, 21, 41, 121, 183, 315, 219, 0},
		        new uint[] {1, 1, 3, 9, 15, 3, 9, 223, 441, 929, 0},
		        new uint[] {1, 1, 7, 9, 3, 5, 93, 57, 253, 457, 0},
		        new uint[] {1, 1, 7, 13, 15, 29, 83, 21, 35, 45, 0},
		        new uint[] {1, 1, 3, 7, 13, 61, 119, 219, 85, 505, 0},
		        new uint[] {1, 1, 3, 3, 17, 13, 35, 197, 291, 109, 0},
		        new uint[] {1, 1, 3, 3, 5, 1, 113, 103, 217, 253, 0},
		        new uint[] {1, 1, 7, 1, 15, 39, 63, 223, 17, 9, 0},
		        new uint[] {1, 3, 7, 1, 17, 29, 67, 103, 495, 383, 0},
		        new uint[] {1, 3, 3, 15, 31, 59, 75, 165, 51, 913, 0},
		        new uint[] {1, 3, 7, 9, 5, 27, 79, 219, 233, 37, 0},
		        new uint[] {1, 3, 5, 15, 1, 11, 15, 211, 417, 811, 0},
		        new uint[] {1, 3, 5, 3, 29, 27, 39, 137, 407, 231, 0},
		        new uint[] {1, 1, 3, 5, 29, 43, 125, 135, 109, 67, 0},
		        new uint[] {1, 1, 1, 5, 11, 39, 107, 159, 323, 381, 0},
		        new uint[] {1, 1, 1, 1, 9, 11, 33, 55, 169, 253, 0},
		        new uint[] {1, 3, 5, 5, 11, 53, 63, 101, 251, 897, 0},
		        new uint[] {1, 3, 7, 1, 25, 15, 83, 119, 53, 157, 0},
		        new uint[] {1, 3, 5, 13, 5, 5, 3, 195, 111, 451, 0},
		        new uint[] {1, 3, 1, 15, 11, 1, 19, 11, 307, 777, 0},
		        new uint[] {1, 3, 7, 11, 5, 5, 17, 231, 345, 981, 0},
		        new uint[] {1, 1, 3, 3, 1, 33, 83, 201, 57, 475, 0},
		        new uint[] {1, 3, 7, 7, 17, 13, 35, 175, 499, 809, 0},
		        new uint[] {1, 1, 5, 3, 3, 17, 103, 119, 499, 865, 0},
		        new uint[] {1, 1, 1, 11, 27, 25, 37, 121, 401, 11, 0},
		        new uint[] {1, 1, 1, 11, 9, 25, 25, 241, 403, 3, 0},
		        new uint[] {1, 1, 1, 1, 11, 1, 39, 163, 231, 573, 0},
		        new uint[] {1, 1, 1, 13, 13, 21, 75, 185, 99, 545, 0},
		        new uint[] {1, 1, 1, 15, 3, 63, 69, 11, 173, 315, 0},
		        new uint[] {1, 3, 5, 15, 11, 3, 95, 49, 123, 765, 0},
		        new uint[] {1, 1, 1, 15, 3, 63, 77, 31, 425, 711, 0},
		        new uint[] {1, 1, 7, 15, 1, 37, 119, 145, 489, 583, 0},
		        new uint[] {1, 3, 5, 15, 3, 49, 117, 211, 165, 323, 0},
		        new uint[] {1, 3, 7, 1, 27, 63, 77, 201, 225, 803, 0},
		        new uint[] {1, 1, 1, 11, 23, 35, 67, 21, 469, 357, 0},
		        new uint[] {1, 1, 7, 7, 9, 7, 25, 237, 237, 571, 0},
		        new uint[] {1, 1, 3, 15, 29, 5, 107, 109, 241, 47, 0},
		        new uint[] {1, 3, 5, 11, 27, 63, 29, 13, 203, 675, 0},
		        new uint[] {1, 1, 3, 9, 9, 11, 103, 179, 449, 263, 0},
		        new uint[] {1, 3, 5, 11, 29, 63, 53, 151, 259, 223, 0},
		        new uint[] {1, 1, 3, 7, 9, 25, 5, 197, 237, 163, 0},
		        new uint[] {1, 3, 7, 13, 5, 57, 67, 193, 147, 241, 0},
		        new uint[] {1, 1, 5, 15, 15, 33, 17, 67, 161, 341, 0},
		        new uint[] {1, 1, 3, 13, 17, 43, 21, 197, 441, 985, 0},
		        new uint[] {1, 3, 1, 5, 15, 33, 33, 193, 305, 829, 0},
		        new uint[] {1, 1, 1, 13, 19, 27, 71, 187, 477, 239, 0},
		        new uint[] {1, 1, 1, 9, 9, 17, 41, 177, 229, 983, 0},
		        new uint[] {1, 3, 5, 9, 15, 45, 97, 205, 43, 767, 0},
		        new uint[] {1, 1, 1, 9, 31, 31, 77, 159, 395, 809, 0},
		        new uint[] {1, 3, 3, 3, 29, 19, 73, 123, 165, 307, 0},
		        new uint[] {1, 3, 1, 7, 5, 11, 77, 227, 355, 403, 0},
		        new uint[] {1, 3, 5, 5, 25, 31, 1, 215, 451, 195, 0},
		        new uint[] {1, 3, 7, 15, 29, 37, 101, 241, 17, 633, 0},
		        new uint[] {1, 1, 5, 1, 11, 3, 107, 137, 489, 5, 0},
		        new uint[] {1, 1, 1, 7, 19, 19, 75, 85, 471, 355, 0},
		        new uint[] {1, 1, 3, 3, 9, 13, 113, 167, 13, 27, 0},
		        new uint[] {1, 3, 5, 11, 21, 3, 89, 205, 377, 307, 0},
		        new uint[] {1, 1, 1, 9, 31, 61, 65, 9, 391, 141, 867, 0},
		        new uint[] {1, 1, 1, 9, 19, 19, 61, 227, 241, 55, 161, 0},
		        new uint[] {1, 1, 1, 11, 1, 19, 7, 233, 463, 171, 1941, 0},
		        new uint[] {1, 1, 5, 7, 25, 13, 103, 75, 19, 1021, 1063, 0},
		        new uint[] {1, 1, 1, 15, 17, 17, 79, 63, 391, 403, 1221, 0},
		        new uint[] {1, 3, 3, 11, 29, 25, 29, 107, 335, 475, 963, 0},
		        new uint[] {1, 3, 5, 1, 31, 33, 49, 43, 155, 9, 1285, 0},
		        new uint[] {1, 1, 5, 5, 15, 47, 39, 161, 357, 863, 1039, 0},
		        new uint[] {1, 3, 7, 15, 1, 39, 47, 109, 427, 393, 1103, 0},
		        new uint[] {1, 1, 1, 9, 9, 29, 121, 233, 157, 99, 701, 0},
		        new uint[] {1, 1, 1, 7, 1, 29, 75, 121, 439, 109, 993, 0},
		        new uint[] {1, 1, 1, 9, 5, 1, 39, 59, 89, 157, 1865, 0},
		        new uint[] {1, 1, 5, 1, 3, 37, 89, 93, 143, 533, 175, 0},
		        new uint[] {1, 1, 3, 5, 7, 33, 35, 173, 159, 135, 241, 0},
		        new uint[] {1, 1, 1, 15, 17, 37, 79, 131, 43, 891, 229, 0},
		        new uint[] {1, 1, 1, 1, 1, 35, 121, 177, 397, 1017, 583, 0},
		        new uint[] {1, 1, 3, 15, 31, 21, 43, 67, 467, 923, 1473, 0},
		        new uint[] {1, 1, 1, 7, 1, 33, 77, 111, 125, 771, 1975, 0},
		        new uint[] {1, 3, 7, 13, 1, 51, 113, 139, 245, 573, 503, 0},
		        new uint[] {1, 3, 1, 9, 21, 49, 15, 157, 49, 483, 291, 0},
		        new uint[] {1, 1, 1, 1, 29, 35, 17, 65, 403, 485, 1603, 0},
		        new uint[] {1, 1, 1, 7, 19, 1, 37, 129, 203, 321, 1809, 0},
		        new uint[] {1, 3, 7, 15, 15, 9, 5, 77, 29, 485, 581, 0},
		        new uint[] {1, 1, 3, 5, 15, 49, 97, 105, 309, 875, 1581, 0},
		        new uint[] {1, 3, 5, 1, 5, 19, 63, 35, 165, 399, 1489, 0},
		        new uint[] {1, 3, 5, 3, 23, 5, 79, 137, 115, 599, 1127, 0},
		        new uint[] {1, 1, 7, 5, 3, 61, 27, 177, 257, 91, 841, 0},
		        new uint[] {1, 1, 3, 5, 9, 31, 91, 209, 409, 661, 159, 0},
		        new uint[] {1, 3, 1, 15, 23, 39, 23, 195, 245, 203, 947, 0},
		        new uint[] {1, 1, 3, 1, 15, 59, 67, 95, 155, 461, 147, 0},
		        new uint[] {1, 3, 7, 5, 23, 25, 87, 11, 51, 449, 1631, 0},
		        new uint[] {1, 1, 1, 1, 17, 57, 7, 197, 409, 609, 135, 0},
		        new uint[] {1, 1, 1, 9, 1, 61, 115, 113, 495, 895, 1595, 0},
		        new uint[] {1, 3, 7, 15, 9, 47, 121, 211, 379, 985, 1755, 0},
		        new uint[] {1, 3, 1, 3, 7, 57, 27, 231, 339, 325, 1023, 0},
		        new uint[] {1, 1, 1, 1, 19, 63, 63, 239, 31, 643, 373, 0},
		        new uint[] {1, 3, 1, 11, 19, 9, 7, 171, 21, 691, 215, 0},
		        new uint[] {1, 1, 5, 13, 11, 57, 39, 211, 241, 893, 555, 0},
		        new uint[] {1, 1, 7, 5, 29, 21, 45, 59, 509, 223, 491, 0},
		        new uint[] {1, 1, 7, 9, 15, 61, 97, 75, 127, 779, 839, 0},
		        new uint[] {1, 1, 7, 15, 17, 33, 75, 237, 191, 925, 681, 0},
		        new uint[] {1, 3, 5, 7, 27, 57, 123, 111, 101, 371, 1129, 0},
		        new uint[] {1, 3, 5, 5, 29, 45, 59, 127, 229, 967, 2027, 0},
		        new uint[] {1, 1, 1, 1, 17, 7, 23, 199, 241, 455, 135, 0},
		        new uint[] {1, 1, 7, 15, 27, 29, 105, 171, 337, 503, 1817, 0},
		        new uint[] {1, 1, 3, 7, 21, 35, 61, 71, 405, 647, 2045, 0},
		        new uint[] {1, 1, 1, 1, 1, 15, 65, 167, 501, 79, 737, 0},
		        new uint[] {1, 1, 5, 1, 3, 49, 27, 189, 341, 615, 1287, 0},
		        new uint[] {1, 1, 1, 9, 1, 7, 31, 159, 503, 327, 1613, 0},
		        new uint[] {1, 3, 3, 3, 3, 23, 99, 115, 323, 997, 987, 0},
		        new uint[] {1, 1, 1, 9, 19, 33, 93, 247, 509, 453, 891, 0},
		        new uint[] {1, 1, 3, 1, 13, 19, 35, 153, 161, 633, 445, 0},
		        new uint[] {1, 3, 5, 15, 31, 5, 87, 197, 183, 783, 1823, 0},
		        new uint[] {1, 1, 7, 5, 19, 63, 69, 221, 129, 231, 1195, 0},
		        new uint[] {1, 1, 5, 5, 13, 23, 19, 231, 245, 917, 379, 0},
		        new uint[] {1, 3, 1, 15, 19, 43, 27, 223, 171, 413, 125, 0},
		        new uint[] {1, 1, 1, 9, 1, 59, 21, 15, 509, 207, 589, 0},
		        new uint[] {1, 3, 5, 3, 19, 31, 113, 19, 23, 733, 499, 0},
		        new uint[] {1, 1, 7, 1, 19, 51, 101, 165, 47, 925, 1093, 0},
		        new uint[] {1, 3, 3, 9, 15, 21, 43, 243, 237, 461, 1361, 0},
		        new uint[] {1, 1, 1, 9, 17, 15, 75, 75, 113, 715, 1419, 0},
		        new uint[] {1, 1, 7, 13, 17, 1, 99, 15, 347, 721, 1405, 0},
		        new uint[] {1, 1, 7, 15, 7, 27, 23, 183, 39, 59, 571, 0},
		        new uint[] {1, 3, 5, 9, 7, 43, 35, 165, 463, 567, 859, 0},
		        new uint[] {1, 3, 3, 11, 15, 19, 17, 129, 311, 343, 15, 0},
		        new uint[] {1, 1, 1, 15, 31, 59, 63, 39, 347, 359, 105, 0},
		        new uint[] {1, 1, 1, 15, 5, 43, 87, 241, 109, 61, 685, 0},
		        new uint[] {1, 1, 7, 7, 9, 39, 121, 127, 369, 579, 853, 0},
		        new uint[] {1, 1, 1, 1, 17, 15, 15, 95, 325, 627, 299, 0},
		        new uint[] {1, 1, 3, 13, 31, 53, 85, 111, 289, 811, 1635, 0},
		        new uint[] {1, 3, 7, 1, 19, 29, 75, 185, 153, 573, 653, 0},
		        new uint[] {1, 3, 7, 1, 29, 31, 55, 91, 249, 247, 1015, 0},
		        new uint[] {1, 3, 5, 7, 1, 49, 113, 139, 257, 127, 307, 0},
		        new uint[] {1, 3, 5, 9, 15, 15, 123, 105, 105, 225, 1893, 0},
		        new uint[] {1, 3, 3, 1, 15, 5, 105, 249, 73, 709, 1557, 0},
		        new uint[] {1, 1, 1, 9, 17, 31, 113, 73, 65, 701, 1439, 0},
		        new uint[] {1, 3, 5, 15, 13, 21, 117, 131, 243, 859, 323, 0},
		        new uint[] {1, 1, 1, 9, 19, 15, 69, 149, 89, 681, 515, 0},
		        new uint[] {1, 1, 1, 5, 29, 13, 21, 97, 301, 27, 967, 0},
		        new uint[] {1, 1, 3, 3, 15, 45, 107, 227, 495, 769, 1935, 0},
		        new uint[] {1, 1, 1, 11, 5, 27, 41, 173, 261, 703, 1349, 0},
		        new uint[] {1, 3, 3, 3, 11, 35, 97, 43, 501, 563, 1331, 0},
		        new uint[] {1, 1, 1, 7, 1, 17, 87, 17, 429, 245, 1941, 0},
		        new uint[] {1, 1, 7, 15, 29, 13, 1, 175, 425, 233, 797, 0},
		        new uint[] {1, 1, 3, 11, 21, 57, 49, 49, 163, 685, 701, 0},
		        new uint[] {1, 3, 3, 7, 11, 45, 107, 111, 379, 703, 1403, 0},
		        new uint[] {1, 1, 7, 3, 21, 7, 117, 49, 469, 37, 775, 0},
		        new uint[] {1, 1, 5, 15, 31, 63, 101, 77, 507, 489, 1955, 0},
		        new uint[] {1, 3, 3, 11, 19, 21, 101, 255, 203, 673, 665, 0},
		        new uint[] {1, 3, 3, 15, 17, 47, 125, 187, 271, 899, 2003, 0},
		        new uint[] {1, 1, 7, 7, 1, 35, 13, 235, 5, 337, 905, 0},
		        new uint[] {1, 3, 1, 15, 1, 43, 1, 27, 37, 695, 1429, 0},
		        new uint[] {1, 3, 1, 11, 21, 27, 93, 161, 299, 665, 495, 0},
		        new uint[] {1, 3, 3, 15, 3, 1, 81, 111, 105, 547, 897, 0},
		        new uint[] {1, 3, 5, 1, 3, 53, 97, 253, 401, 827, 1467, 0},
		        new uint[] {1, 1, 1, 5, 19, 59, 105, 125, 271, 351, 719, 0},
		        new uint[] {1, 3, 5, 13, 7, 11, 91, 41, 441, 759, 1827, 0},
		        new uint[] {1, 3, 7, 11, 29, 61, 61, 23, 307, 863, 363, 0},
		        new uint[] {1, 1, 7, 1, 15, 35, 29, 133, 415, 473, 1737, 0},
		        new uint[] {1, 1, 1, 13, 7, 33, 35, 225, 117, 681, 1545, 0},
		        new uint[] {1, 1, 1, 3, 5, 41, 83, 247, 13, 373, 1091, 0},
		        new uint[] {1, 3, 1, 13, 25, 61, 71, 217, 233, 313, 547, 0},
		        new uint[] {1, 3, 1, 7, 3, 29, 3, 49, 93, 465, 15, 0},
		        new uint[] {1, 1, 1, 9, 17, 61, 99, 163, 129, 485, 1087, 0},
		        new uint[] {1, 1, 1, 9, 9, 33, 31, 163, 145, 649, 253, 0},
		        new uint[] {1, 1, 1, 1, 17, 63, 43, 235, 287, 111, 567, 0},
		        new uint[] {1, 3, 5, 13, 29, 7, 11, 69, 153, 127, 449, 0},
		        new uint[] {1, 1, 5, 9, 11, 21, 15, 189, 431, 493, 1219, 0},
		        new uint[] {1, 1, 1, 15, 19, 5, 47, 91, 399, 293, 1743, 0},
		        new uint[] {1, 3, 3, 11, 29, 53, 53, 225, 409, 303, 333, 0},
		        new uint[] {1, 1, 1, 15, 31, 31, 21, 81, 147, 287, 1753, 0},
		        new uint[] {1, 3, 5, 5, 5, 63, 35, 125, 41, 687, 1793, 0},
		        new uint[] {1, 1, 1, 9, 19, 59, 107, 219, 455, 971, 297, 0},
		        new uint[] {1, 1, 3, 5, 3, 51, 121, 31, 245, 105, 1311, 0},
		        new uint[] {1, 3, 1, 5, 5, 57, 75, 107, 161, 431, 1693, 0},
		        new uint[] {1, 3, 1, 3, 19, 53, 27, 31, 191, 565, 1015, 0},
		        new uint[] {1, 3, 5, 13, 9, 41, 35, 249, 287, 49, 123, 0},
		        new uint[] {1, 1, 5, 7, 27, 17, 21, 3, 151, 885, 1165, 0},
		        new uint[] {1, 1, 7, 1, 15, 17, 65, 139, 427, 339, 1171, 0},
		        new uint[] {1, 1, 1, 5, 23, 5, 9, 89, 321, 907, 391, 0},
		        new uint[] {1, 1, 7, 9, 15, 1, 77, 71, 87, 701, 917, 0},
		        new uint[] {1, 1, 7, 1, 17, 37, 115, 127, 469, 779, 1543, 0},
		        new uint[] {1, 3, 7, 3, 5, 61, 15, 37, 301, 951, 1437, 0},
		        new uint[] {1, 1, 1, 13, 9, 51, 127, 145, 229, 55, 1567, 0},
		        new uint[] {1, 3, 7, 15, 19, 47, 53, 153, 295, 47, 1337, 0},
		        new uint[] {1, 3, 3, 5, 11, 31, 29, 133, 327, 287, 507, 0},
		        new uint[] {1, 1, 7, 7, 25, 31, 37, 199, 25, 927, 1317, 0},
		        new uint[] {1, 1, 7, 9, 3, 39, 127, 167, 345, 467, 759, 0},
		        new uint[] {1, 1, 1, 1, 31, 21, 15, 101, 293, 787, 1025, 0},
		        new uint[] {1, 1, 5, 3, 11, 41, 105, 109, 149, 837, 1813, 0},
		        new uint[] {1, 1, 3, 5, 29, 13, 19, 97, 309, 901, 753, 0},
		        new uint[] {1, 1, 7, 1, 19, 17, 31, 39, 173, 361, 1177, 0},
		        new uint[] {1, 3, 3, 3, 3, 41, 81, 7, 341, 491, 43, 0},
		        new uint[] {1, 1, 7, 7, 31, 35, 29, 77, 11, 335, 1275, 0},
		        new uint[] {1, 3, 3, 15, 17, 45, 19, 63, 151, 849, 129, 0},
		        new uint[] {1, 1, 7, 5, 7, 13, 47, 73, 79, 31, 499, 0},
		        new uint[] {1, 3, 1, 11, 1, 41, 59, 151, 247, 115, 1295, 0},
		        new uint[] {1, 1, 1, 9, 31, 37, 73, 23, 295, 483, 179, 0},
		        new uint[] {1, 3, 1, 15, 13, 63, 81, 27, 169, 825, 2037, 0},
		        new uint[] {1, 3, 5, 15, 7, 11, 73, 1, 451, 101, 2039, 0},
		        new uint[] {1, 3, 5, 3, 13, 53, 31, 137, 173, 319, 1521, 0},
		        new uint[] {1, 3, 1, 3, 29, 1, 73, 227, 377, 337, 1189, 0},
		        new uint[] {1, 3, 3, 13, 27, 9, 31, 101, 229, 165, 1983, 0},
		        new uint[] {1, 3, 1, 13, 13, 19, 19, 111, 319, 421, 223, 0},
		        new uint[] {1, 1, 7, 15, 25, 37, 61, 55, 359, 255, 1955, 0},
		        new uint[] {1, 1, 5, 13, 17, 43, 49, 215, 383, 915, 51, 0},
		        new uint[] {1, 1, 3, 1, 3, 7, 13, 119, 155, 585, 967, 0},
		        new uint[] {1, 3, 1, 13, 1, 63, 125, 21, 103, 287, 457, 0},
		        new uint[] {1, 1, 7, 1, 31, 17, 125, 137, 345, 379, 1925, 0},
		        new uint[] {1, 1, 3, 5, 5, 25, 119, 153, 455, 271, 2023, 0},
		        new uint[] {1, 1, 7, 9, 9, 37, 115, 47, 5, 255, 917, 0},
		        new uint[] {1, 3, 5, 3, 31, 21, 75, 203, 489, 593, 1, 0},
		        new uint[] {1, 3, 7, 15, 19, 63, 123, 153, 135, 977, 1875, 0},
		        new uint[] {1, 1, 1, 1, 5, 59, 31, 25, 127, 209, 745, 0},
		        new uint[] {1, 1, 1, 1, 19, 45, 67, 159, 301, 199, 535, 0},
		        new uint[] {1, 1, 7, 1, 31, 17, 19, 225, 369, 125, 421, 0},
		        new uint[] {1, 3, 3, 11, 7, 59, 115, 197, 459, 469, 1055, 0},
		        new uint[] {1, 3, 1, 3, 27, 45, 35, 131, 349, 101, 411, 0},
		        new uint[] {1, 3, 7, 11, 9, 3, 67, 145, 299, 253, 1339, 0},
		        new uint[] {1, 3, 3, 11, 9, 37, 123, 229, 273, 269, 515, 0},
		        new uint[] {1, 3, 7, 15, 11, 25, 75, 5, 367, 217, 951, 0},
		        new uint[] {1, 1, 3, 7, 9, 23, 63, 237, 385, 159, 1273, 0},
		        new uint[] {1, 1, 5, 11, 23, 5, 55, 193, 109, 865, 663, 0},
		        new uint[] {1, 1, 7, 15, 1, 57, 17, 141, 51, 217, 1259, 0},
		        new uint[] {1, 1, 3, 3, 15, 7, 89, 233, 71, 329, 203, 0},
		        new uint[] {1, 3, 7, 11, 11, 1, 19, 155, 89, 437, 573, 0},
		        new uint[] {1, 3, 1, 9, 27, 61, 47, 109, 161, 913, 1681, 0},
		        new uint[] {1, 1, 7, 15, 1, 33, 19, 15, 23, 913, 989, 0},
		        new uint[] {1, 3, 1, 1, 25, 39, 119, 193, 13, 571, 157, 0},
		        new uint[] {1, 1, 7, 13, 9, 55, 59, 147, 361, 935, 515, 0},
		        new uint[] {1, 1, 1, 9, 7, 59, 67, 117, 71, 855, 1493, 0},
		        new uint[] {1, 3, 1, 3, 13, 19, 57, 141, 305, 275, 1079, 0},
		        new uint[] {1, 1, 1, 9, 17, 61, 33, 7, 43, 931, 781, 0},
		        new uint[] {1, 1, 3, 1, 11, 17, 21, 97, 295, 277, 1721, 0},
		        new uint[] {1, 3, 1, 13, 15, 43, 11, 241, 147, 391, 1641, 0},
		        new uint[] {1, 1, 1, 1, 1, 19, 37, 21, 255, 263, 1571, 0},
		        new uint[] {1, 1, 3, 3, 23, 59, 89, 17, 475, 303, 757, 543, 0},
		        new uint[] {1, 3, 3, 9, 11, 55, 35, 159, 139, 203, 1531, 1825, 0},
		        new uint[] {1, 1, 5, 3, 17, 53, 51, 241, 269, 949, 1373, 325, 0},
		        new uint[] {1, 3, 7, 7, 5, 29, 91, 149, 239, 193, 1951, 2675, 0},
		        new uint[] {1, 3, 5, 1, 27, 33, 69, 11, 51, 371, 833, 2685, 0},
		        new uint[] {1, 1, 1, 15, 1, 17, 35, 57, 171, 1007, 449, 367, 0},
		        new uint[] {1, 1, 1, 7, 25, 61, 73, 219, 379, 53, 589, 4065, 0},
		        new uint[] {1, 3, 5, 13, 21, 29, 45, 19, 163, 169, 147, 597, 0},
		        new uint[] {1, 1, 5, 11, 21, 27, 7, 17, 237, 591, 255, 1235, 0},
		        new uint[] {1, 1, 7, 7, 17, 41, 69, 237, 397, 173, 1229, 2341, 0},
		        new uint[] {1, 1, 3, 1, 1, 33, 125, 47, 11, 783, 1323, 2469, 0},
		        new uint[] {1, 3, 1, 11, 3, 39, 35, 133, 153, 55, 1171, 3165, 0},
		        new uint[] {1, 1, 5, 11, 27, 23, 103, 245, 375, 753, 477, 2165, 0},
		        new uint[] {1, 3, 1, 15, 15, 49, 127, 223, 387, 771, 1719, 1465, 0},
		        new uint[] {1, 1, 1, 9, 11, 9, 17, 185, 239, 899, 1273, 3961, 0},
		        new uint[] {1, 1, 3, 13, 11, 51, 73, 81, 389, 647, 1767, 1215, 0},
		        new uint[] {1, 3, 5, 15, 19, 9, 69, 35, 349, 977, 1603, 1435, 0},
		        new uint[] {1, 1, 1, 1, 19, 59, 123, 37, 41, 961, 181, 1275, 0},
		        new uint[] {1, 1, 1, 1, 31, 29, 37, 71, 205, 947, 115, 3017, 0},
		        new uint[] {1, 1, 7, 15, 5, 37, 101, 169, 221, 245, 687, 195, 0},
		        new uint[] {1, 1, 1, 1, 19, 9, 125, 157, 119, 283, 1721, 743, 0},
		        new uint[] {1, 1, 7, 3, 1, 7, 61, 71, 119, 257, 1227, 2893, 0},
		        new uint[] {1, 3, 3, 3, 25, 41, 25, 225, 31, 57, 925, 2139, 0}
		    };
			return result;
		}		
		#endregion
	}	

}