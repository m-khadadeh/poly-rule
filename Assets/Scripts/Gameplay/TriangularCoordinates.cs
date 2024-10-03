using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TriangularCoordinates : MonoBehaviour
{
    public static TriangularCoordinates instance;

    public int section;
    public float radius;
    public bool showCoords;

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (section == 0) {
            return;
        }
		Mesh grid = new Mesh();

		generateGrid(ref grid);
        Gizmos.color = new Color(0,0,1,0.3f);
        Gizmos.DrawWireMesh(grid);

        if (showCoords)
        {
            for (int i = -section; i <= section; i++)
            {
                int np_col_i = 2 * section + 1 - Mathf.Abs(i);
                int row_min = -section;
                if (i < 0)
                    row_min += Mathf.Abs(i);
                int row_max = row_min + np_col_i - 1;
                for (int j = row_min; j <= row_max; j++)
                {
                    Vector3 pointPos = triangleToEuclidean(j, i);
                    Gizmos.DrawSphere(pointPos, 0.08f);
                    Handles.Label(pointPos+(Vector3.up*0.2f)+(Vector3.right*0.1f), "(" + j + ", " + i + ")");
                }
            }
        }
    }
#endif
    // shoveled from http://www.voidinspace.com/2014/07/project-twa-part-1-generating-a-hexagonal-tile-and-its-triangular-grid/
    void generateGrid(ref Mesh rMesh)
	{
		float sin60 = Mathf.Sin(Mathf.PI / 3);
		float inv_tan60 = 1 / Mathf.Tan(Mathf.PI / 3);
		float RdS = radius;

		int num_vertices = 1;
		for (int i = 1; i <= section; i++)
			num_vertices += i * 6;

		int vertices_index = 0;
		Vector3[] vertices = new Vector3[num_vertices];

		int num_indices = 0;
		for (int i = 1; i <= section; i++)
			num_indices += 36 * i - 18;

		int indices_index = 0;
		int[] indices = new int[num_indices];

		int current_num_points = 0;
		int prev_row_num_points = 0;

		int np_col_0 = 2 * section + 1;
		int col_min = -section;
		int col_max = section;

		for (int itC = col_min; itC <= col_max; itC++)
		{
			float x = sin60 * RdS * itC;
			int np_col_i = np_col_0 - Mathf.Abs(itC);

			int row_min = -section;
			if (itC < 0)
				row_min += Mathf.Abs(itC);

			int row_max = row_min + np_col_i - 1;

			current_num_points += np_col_i;

			for (int itR = row_min; itR <= row_max; itR++)
			{
				float y = inv_tan60 * x + RdS * itR;

				vertices[vertices_index] = new Vector3(x + 2 * Mathf.Floor(transform.position.x), y + 2 * Mathf.Floor(transform.position.y), 0);

				if (vertices_index < (current_num_points - 1))
				{
					if (itC >= col_min && itC < col_max)
					{
						int pad_left = 0;
						if (itC < 0)
							pad_left = 1;

						indices[indices_index] = vertices_index;
						indices_index++;

						indices[indices_index] = vertices_index + 1;
						indices_index++;

						indices[indices_index] = vertices_index + np_col_i + pad_left;
						indices_index++;
					}

					if (itC > col_min && itC <= col_max)
					{
						int pad_right = 0;
						if (itC > 0)
							pad_right = 1;

						indices[indices_index] = vertices_index + 1;
						indices_index++;

						indices[indices_index] = vertices_index;
						indices_index++;

						indices[indices_index] = vertices_index - prev_row_num_points + pad_right;
						indices_index++;
					}
				}

				vertices_index++;
			}

			prev_row_num_points = np_col_i;
		}

		rMesh.vertices = vertices;
		rMesh.triangles = indices;
		rMesh.RecalculateNormals();
	}

    public Vector3 triangleToEuclidean(int row, int col)
    {
        float sin60 = Mathf.Sin(Mathf.PI / 3);
        float inv_tan60 = 1 / Mathf.Tan(Mathf.PI / 3);
        float RdS = radius;

        float x = sin60 * RdS * col;
        float y = inv_tan60 * x + RdS * row;

        return new Vector3(x, y, 0);
    }

    public bool isInLine(Vector2 lineStart, Vector2 direction, Vector2 endPoint)
    {
        Vector2 startToEnd = endPoint - lineStart;
        int multiplier = (startToEnd.x == 0) ? (int)Mathf.Abs(startToEnd.y) : (int)Mathf.Abs(startToEnd.x);
        if (direction * multiplier == startToEnd) return true;
        else return false;
    }
}