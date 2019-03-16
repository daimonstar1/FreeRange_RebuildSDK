using System;
using System.Collections.Generic;
using UnityEngine;

public static class TriangleUtil {
    // from http://www.ics.uci.edu/~eppstein/junkyard/circumcenter.html ?
    public static void TriCircumcenter( Vector2 a, Vector2 b, Vector2 c, out Vector2 center, out float sqRadius ) {
        Vector2 ba = b - a;
        Vector2 ca = c - a;
        float balength = ba.sqrMagnitude;
        float calength = ca.sqrMagnitude;

        float denominator = 0.5f / (ba.x * ca.y - ba.y * ca.x);
        float xcirca = (ca.y * balength - ba.y * calength) * denominator;
        float ycirca = (ba.x * calength - ca.x * balength) * denominator;

        // original version just did this, center offset from point a; compute radius yourself
        center = new Vector2( xcirca, ycirca );
        sqRadius = center.sqrMagnitude;
        center += a;
    }

    public class Tri {
        public int[] verts = new int[3];
        public Tri[] neighbors = new Tri[3];

        public Tri( int a, int b, int c ) {
            verts[0] = a;
            verts[1] = b;
            verts[2] = c;
        }

        public override string ToString() {
            return "triangle (" + verts[0] + ", " + verts[1] + ", " + verts[2] + ")";
        }

        internal int GetEdgeFacing( Tri t ) {
            for( int i = 0; i < 3; i++ ) {
                if( neighbors[i] == t ) return i;
            }
            Debug.LogWarning( this + " has no facing edge for " + t );
            return -1;
        }

        internal void AddNeighbor( int a, int b, Tri add ) {
            if( verts[0] == a ) {
                if( verts[1] == b ) {
                    neighbors[2] = add;
                } else if( verts[2] == b ) {
                    neighbors[1] = add;
                } else {
                    Debug.LogWarning( this + " has no neighbor (" + a + ", " + b + ") for " + add );
                }
            } else if( verts[1] == a ) {
                if( verts[0] == b ) {
                    neighbors[2] = add;
                } else if( verts[2] == b ) {
                    neighbors[0] = add;
                } else {
                    Debug.LogWarning( this + " has no neighbor (" + a + ", " + b + ") for " + add );
                }
            } else if( verts[2] == a ) {
                if( verts[0] == b ) {
                    neighbors[1] = add;
                } else if( verts[1] == b ) {
                    neighbors[0] = add;
                } else {
                    Debug.LogWarning( this + " has no neighbor (" + a + ", " + b + ") for " + add );
                }
            }
        }
    }

    // we also store the opposite triangle to the edge
    public class Edge {
        public int a, b;
        public Tri tri;

        public Edge( int a, int b, Tri tri ) {
            this.a = a;
            this.b = b;
            this.tri = tri;
        }
    }

    public class Circle {
        public Vector2 center;
        public float sqRadius;

        public Circle( Vector2 center, float sqRadius ) {
            this.center = center;
            this.sqRadius = sqRadius;
        }
        public bool Contains( Vector2 pt ) {
            if( (center - pt).sqrMagnitude <= sqRadius ) {
                return true;
            }
            return false;
        }
    }

    // https://github.com/jmespadero/pyDelaunay2D/blob/master/delaunay2D.py
    // https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
    class Triangulator {
        List<Vector2> points;
        List<Tri> tris;
        List<Circle> circles;

        List<Tri> badTris;

        public Triangulator( List<Vector2> points ) {
            this.points = new List<Vector2>( points );
            tris = new List<Tri>();
            circles = new List<Circle>();
        }

        public void Setup() {
            Vector2 min = points[0];
            Vector2 max = points[0];
            for( int i = 1; i < points.Count; i++ ) {
                if( points[i].x < min.x ) min.x = points[i].x;
                if( points[i].y < min.y ) min.y = points[i].y;
                if( points[i].x > max.x ) max.x = points[i].x;
                if( points[i].y > max.y ) max.y = points[i].y;
            }
            min -= Vector2.one;
            max += Vector2.one;

            points.Add( min );
            points.Add( new Vector2( max.x, min.y ) );
            points.Add( max );
            points.Add( new Vector2( min.x, max.y ) );

            int fakeTriIdx = points.Count-4;
            AddTri( new Tri( fakeTriIdx, fakeTriIdx + 1, fakeTriIdx + 2 ) );
            AddTri( new Tri( fakeTriIdx, fakeTriIdx + 2, fakeTriIdx + 3 ) );
            tris[0].neighbors[1] = tris[1];
            tris[1].neighbors[2] = tris[0];

            badTris = new List<Tri>();
        }

        private void AddTri( Tri tri ) {
            tris.Add( tri );
            Vector2 center;
            float radius;
            TriCircumcenter( points[tri.verts[0]], points[tri.verts[1]], points[tri.verts[2]], out center, out radius );
            circles.Add( new Circle( center, radius ) );
        }

        public void AddPoint( int p ) {
            for( int i = 0; i < tris.Count; i++ ) {
                if( circles[i].Contains( points[p] ) ) {
                    // triangle i is bad, add to badTris
                    badTris.Add( tris[i] );
                }
            }
            if( badTris.Count == 0 ) {
                Debug.LogError( "There are no bad triangles!  This should never happen!" );
                return;
            }
            List<Edge> polygon = new List<Edge>();
            // add the edges of all bad tris to polygon
            Tri t = badTris[0];
            int edge = 0;
            int alert = 0;
            do {
                Tri opposite = t.neighbors[edge];
                if( !badTris.Contains( opposite ) ) {
                    polygon.Add( GetOppositeEdge( t, edge ) );
                    edge = (edge + 1) % 3;
                } else {
                    // move to the opposite triangle, and slide to the next edge
                    edge = opposite.GetEdgeFacing( t );
                    edge = (edge + 1) % 3;
                    t = opposite;
                }
                alert++;
                if( alert > 500 ) {
                    Debug.LogWarning( "alert!" );
                    break;
                }
            } while( polygon.Count == 0 || polygon[0].a != polygon[polygon.Count - 1].b );

            // remove bad tris
            foreach( Tri bad in badTris ) {
                RemoveTri( bad );
            }
            badTris.Clear();

            // then reconnect boundary polygon to point
            int newTriCount = polygon.Count;
            for( int i = 0; i < newTriCount; i++ ) {
                Edge e = polygon[i];
                Tri add = new Tri( p, e.a, e.b );
                if( e.tri != null ) {
                    add.neighbors[0] = e.tri;
                    e.tri.AddNeighbor( e.a, e.b, add );
                }
                AddTri( add );
                // cheese: store add in edge so we can get at it later to reconnect
                e.tri = add;
            }
            // and reconnect
            for( int i = 0; i < newTriCount; i++ ) {
                Edge e = polygon[i];
                e.tri.neighbors[1] = polygon[(i + 1) % newTriCount].tri;
                e.tri.neighbors[2] = polygon[(i + newTriCount-1) % newTriCount].tri;
            }
        }

        private void RemoveTri( Tri bad ) {
            int idx = tris.IndexOf( bad );
            tris.RemoveAt( idx );
            circles.RemoveAt( idx );
            for( int i = 0; i < 3; i++ ) {
                Tri adj = bad.neighbors[i];
                if( adj != null ) {
                    for( int j = 0; j < 3; j++ ) {
                        if( adj.neighbors[j] == bad ) adj.neighbors[j] = null;
                    }
                }
            }
        }

        private Edge GetOppositeEdge( Tri t, int edge ) {
            int a = t.verts[(edge + 1) % 3];
            int b = t.verts[(edge + 2) % 3];
            Tri opp = t.neighbors[edge];

            return new Edge( a, b, opp );
        }

        public void Cleanup() {
            points.RemoveRange( points.Count - 4, 4 );
            int lastPoint = points.Count;

            for( int i = tris.Count-1; i >=0;  i-- ) {
                if( tris[i].verts[0] >= lastPoint || tris[i].verts[1] >= lastPoint || tris[i].verts[2] >= lastPoint ) {
                    tris.RemoveAt( i );
                    circles.RemoveAt( i );
                }
            }
        }

        public List<Tri> GetTris() {
            return tris;
        }
    }

    // delaunay triangulate the list of points, return list of tris
    public static List<Tri> DoTriangulation( List<Vector2> points ) {
        Triangulator d = new Triangulator( points );

        // create frame triangles
        d.Setup();

        for( int i = 0; i < points.Count; i++ ) {
            d.AddPoint( i );
        }

        d.Cleanup();

        return d.GetTris();
    }

    public static List<List<int>> GetAdjacency( List<Vector2> points ) {
        List<Tri> tris = DoTriangulation( points );
        List<List<int>> output = new List<List<int>>();
        for( int i = 0; i < points.Count; i++ ) {
            output.Add( new List<int>() );
        }
        foreach( Tri tri in tris ) {
            for( int i = 0; i < 3; i++ ) {
                int point = tri.verts[i];
                List<int> adjlist = output[point];
                int other;
                other = tri.verts[(i + 1) % 3];
                if( !adjlist.Contains( other ) ) adjlist.Add( other );
                other = tri.verts[(i + 2) % 3];
                if( !adjlist.Contains( other ) ) adjlist.Add( other );
            }
        }
        return output;
    }

}