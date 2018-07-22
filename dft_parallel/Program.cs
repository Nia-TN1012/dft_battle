using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;

namespace DftParallel {
	class MainClass {

        /// <summary>
        /// 離散フーリエ変換
        /// </summary>
        /// <returns>出力信号</returns>
        /// <param name="x">入力信号</param>
        public static Complex[] DFT( double[] x ) {
            double w = 2.0 * Math.PI / x.Length;
            Complex[] fx = new Complex[x.Length];
            for (int k = 0; k < x.Length; k ++ ) {
                var fxk = x.Select( ( xm, m ) => ( xm, m ) )
                           .Aggregate( Complex.Zero, ( tmp, cur ) => {
			                    tmp += new Complex( cur.xm * Math.Cos( w * k * cur.m ), -cur.xm * Math.Sin( w * k * cur.m ) );
			                    return tmp;
			                } );
                fx[k] = fxk;
            }
            return fx;
        }

        /// <summary>
		/// 離散フーリエ変換（並列）
		/// </summary>
		/// <returns>出力信号</returns>
		/// <param name="x">入力信号</param>
		public static Complex[] ParallelDFT( double[] x ) {
			double w = 2.0 * Math.PI / x.Length;
			Complex[] fx = new Complex[x.Length];
			Parallel.For( 0, x.Length, ( k ) => {
				var fxk = x.Select( ( xm, m ) => (xm, m) )
						   .Aggregate( Complex.Zero, ( tmp, cur ) => {
							   tmp += new Complex( cur.xm * Math.Cos( w * k * cur.m ), -cur.xm * Math.Sin( w * k * cur.m ) );
							   return tmp;
						   } );
				fx[k] = fxk;
			} );
			return fx;
		}

        /// <summary>
        /// 複素数のシーケンス作成
        /// </summary>
        /// <returns>複素数のシーケンス</returns>
        /// <param name="ts">時間の刻み</param>
        /// <param name="func">関数</param>
        public static IEnumerable<double> GenenateComplexTimeSequence( double ts, Func<double, double> func ) {
            double t = 0.0;
            while( true ) {
                yield return func( t );
                t += ts;
            }
        }

        // cos関数の離散フーリエ変換
		public static void Main( string[] args ) {
            var x = GenenateComplexTimeSequence( 0.0001, t => Math.Cos( 2.0 * Math.PI * t ) ).Take( 4096 ).ToArray();

            int count = 10;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for( int i = 0; i < count; i++ ) {
                DFT( x );
            }
            sw.Stop();
            Console.WriteLine( $"DFT: {sw.Elapsed.TotalMilliseconds / count:F6} msec." );

            sw.Reset();
            sw.Start();
			for ( int i = 0; i < count; i++ ) {
				ParallelDFT( x );
			}
			sw.Stop();
            Console.WriteLine( $"ParallelDFT: {sw.Elapsed.TotalMilliseconds / count:F6} msec." );
		}
	}
}
