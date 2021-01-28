using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _blockPrefab = null;
    [SerializeField] Text _scoreText = null;
    [SerializeField] TextMeshProUGUI _startText = null;

    private const int StageHeight = 22;
    private const int StageWidth = 12;

    private List<List<Block>> _stage = new List<List<Block>>();

    private Vector3[][] minoRelativePos = new Vector3[7][];

    private Mino[] _mino = new Mino[(int)Mino.MinoType.EnumMax];

    private Mino.MinoType _minoType = default;
    private int _rotate = default;
    private Vector3 _pos = default;

    // タイマー変数
    private float _countTime = default;
    private float _countStart = default;

    // スタートの表示変数
    private int _countNumber = 0;

    private bool _isGameOver = false;

    private float _minoDefaultSpeed = 1f;
    private float _minoSpeed = default;

    // スコア
    private int _score = default;

    // スタートフラグ
    private bool _startFlag = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // ステージ
        InitializeStage();
        InstanceStage();

        InitializeMino();
        CreateMino();

        _scoreText.text = "SCORE:0";

    }

    // Update is called once per frame
    void Update()
    {
        if (!_startFlag)
        {
            CountTimer();
            return;
        }

        if (_isGameOver)
        {
            GameOverEffect();
            GameOver();
            if (Input.GetKeyDown(KeyCode.Space)) { SceneManager.LoadScene("Title"); }
            return;
        }

        _minoSpeed = _minoDefaultSpeed;

        // 瞬間
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { LeftMove(); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { RightMove(); }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { RotateMino(); }
        if (Input.GetKey(KeyCode.DownArrow)) { _minoSpeed *= 0.02f; }

        // 1秒間に1回secを呼ぶ
        _countTime += Time.deltaTime;
        if (_countTime > _minoSpeed)
        {
            Tick1Sec();
            _countTime = 0f;
        }

        RefreshStage();
        
    }

    private void InitializeStage()
    {
        // 初期化のみ🅆から始まっているから注意してね～
        for(int w = 0; w < StageWidth; w++)
        {
            List<Block> tmpList = new List<Block>();
            for (int h = 0; h < StageHeight; h++)
            {
                Block tmpBlock = new Block();
                tmpBlock.obj = new Renderer();

                if (IsWall(w,h))
                {
                    tmpBlock.type = Mino.MinoType.Wall;
                }
                else
                {
                    tmpBlock.type = Mino.MinoType.Empty;
                }
                tmpList.Add(tmpBlock);
            };
            _stage.Add(tmpList);
        };
    }

    private void InstanceStage()
    {
        for (int h = 0; h < StageHeight; h++)
        {
            for (int w = 0; w < StageWidth; w++)
            {
                // インスタンス化              what        where(m[メートル])  rotate
                // as キャスト クラスでやろうね！！
                _stage[w][h].obj = Instantiate(_blockPrefab, new Vector3(w, h, 0), Quaternion.identity).GetComponent<Renderer>();
                if (_stage[w][h].type == Mino.MinoType.Wall)
                {
                    // 実体が出てきてから色を変えているためNULLExceptionが出ない
                    _stage[w][h].obj.material.color = new Color32(0,0,0,0);
                }
                else
                {
                    _stage[w][h].obj.gameObject.SetActive(false);
                }

            }
        }
    }

    private void InitializeMino()
    {

        _mino[(int)Mino.MinoType.Empty] = new Mino(0, new Color32(0, 255, 255, 255), 0, 0, 0, 0, 0, 0);
        _mino[(int)Mino.MinoType.Wall] = new Mino(0, new Color32(0, 255, 255, 255), 0, 0, 0, 0, 0, 0);
        _mino[(int)Mino.MinoType.I] = new Mino(2, new Color32(0, 255, 255, 255), 0,1, 0,-1, 0,-2);
        _mino[(int)Mino.MinoType.J] = new Mino(4, new Color32(0, 0, 255, 255), 0,1, 0,-1, -1,-1);
        _mino[(int)Mino.MinoType.L] = new Mino(4, new Color32(255, 165, 0, 255), 0,1, 0,-1, 1,-1);
        _mino[(int)Mino.MinoType.S] = new Mino(2, new Color32(0, 255, 0, 255), -1,-1, 0 ,-1, 1,0);
        _mino[(int)Mino.MinoType.Z] = new Mino(2, new Color32(255, 0, 0, 255), 0,-1, -1,0, 1,-1);
        _mino[(int)Mino.MinoType.O] = new Mino(1, new Color32(255, 255, 0, 255), 0,-1, -1,-1, -1,0);
        _mino[(int)Mino.MinoType.T] = new Mino(4, new Color32(128, 0, 128, 255),-1,0, 1,0,0,1);

    }

    // MINO情報を作成
    private void CreateMino()
    {

        _pos.x = StageWidth / 2;
        _pos.y = StageHeight - 3;

        _minoType = (Mino.MinoType)Random.Range(2,8);
        _rotate = Random.Range(2,6);

        if (!IsCanPutMino((int)_pos.x, (int)_pos.y, _rotate))
        {
            _isGameOver = true;
        }
        else
        {
            DrawBlock();
        }
    }

    // ブロックの描画
    private void DrawBlock()
    {

        _stage[(int)_pos.x][(int)_pos.y].obj.material.color = _mino[(int)_minoType].minoColor;

        _stage[(int)_pos.x][(int)_pos.y].type = _minoType;

        foreach (var minoT in _mino[(int)_minoType].relativePos)
        {

            int dx = (int)minoT.x;
            int dy = (int)minoT.y;

            // Oミノ
            int r = _rotate % _mino[(int)_minoType].minoRotateMax;

            // 回転行列
            for (int i = 0; i < r; i++)
            {
                // 現在地
                int nx = dx;
                int ny = dy;

                dx = ny;
                dy = -nx;

            }

            _stage[(int)(_pos.x + dx)][(int)(_pos.y + dy)].obj.material.color = _mino[(int)_minoType].minoColor;
            _stage[(int)(_pos.x + dx)][(int)(_pos.y + dy)].type = _minoType;

        }
        
    }

    // ブロックの消す
    private void EraseBlock()
    {

        _stage[(int)_pos.x][(int)_pos.y].type = Mino.MinoType.Empty;

        foreach (var minoT in _mino[(int)_minoType].relativePos)
        {
            int dx = (int)minoT.x;
            int dy = (int)minoT.y;

            // Oミノ
            int r = _rotate % _mino[(int)_minoType].minoRotateMax;

            // 回転行列
            for (int i = 0; i < r; i++)
            {
                // 現在地
                int nx = dx;
                int ny = dy;

                dx = ny;
                dy = -nx;

            }

            _stage[(int)(_pos.x + dx)][(int)(_pos.y + dy)].type = Mino.MinoType.Empty;
        }
    }

    // rot = rotate
    private bool IsCanPutMino(int x,int y,int rot)
    {

        if (_stage[x][y].type != Mino.MinoType.Empty)
        {
            return false;
        }

        foreach (var minoT in _mino[(int)_minoType].relativePos)
        {
            int dx = (int)minoT.x;
            int dy = (int)minoT.y;

            // Oミノ
            int r = rot % _mino[(int)_minoType].minoRotateMax;

            // 回転行列
            for (int i = 0; i < r; i++)
            {
                // 現在地
                int nx = dx;
                int ny = dy;

                dx = ny;
                dy = -nx;

            }
            
            if (_stage[x + dx][y + dy].type != Mino.MinoType.Empty)
            {
                return false;
            }
        }

        return true;
    }

    private void GameOver()
    {

        Debug.Log("gameover");

    }

    private void Tick1Sec()
    {
        EraseBlock();

        if (IsCanPutMino((int)_pos.x, (int)_pos.y - 1, _rotate))
        {
            _pos.y -= 1f;
            DrawBlock();
        }
        else
        {
            // drawでstaeg更新もやってるからね！

            DrawBlock();
            DeleteLine();
            CreateMino();
            RefreshStage();
        }
    }

    private void LeftMove()
    {
        EraseBlock();

        if (IsCanPutMino((int)_pos.x - 1, (int)_pos.y, _rotate))
        {
            _pos.x -= 1f;
            DrawBlock();
        }
        else
        {
            DrawBlock();
        }
    }

    private void RightMove()
    {
        EraseBlock();

        if (IsCanPutMino((int)_pos.x + 1, (int)_pos.y, _rotate))
        {
            _pos.x += 1f;
            DrawBlock();
        }
        else
        {
            DrawBlock();
        }
    }

    private void RotateMino()
    {
        EraseBlock();

        if (IsCanPutMino((int)_pos.x, (int)_pos.y, _rotate + 1))
        {
            _rotate++;
            DrawBlock();
        }
        else
        {
            DrawBlock();
        }
    }

    private void DeleteLine()
    {
        int deleteLineCount = 0;

        for (int h = 1; h < StageHeight - 1; h++)
        {
            bool lineFlag = true;
            for (int w = 1; w < StageWidth - 1; w++)
            {
                if (_stage[w][h].type == Mino.MinoType.Empty)
                {
                    lineFlag = false;
                    break;
                }
            }
            if (lineFlag)
            {
                // 自分自身を1個上のブロックにすする
                // 消したブロックlineの一つ上のブロックラインを参照している
                for (int dh = h; dh < StageHeight - 2; dh++)
                {
                    // 枠を考慮して1から始まru
                    for (int dw = 1; dw < StageWidth - 1; dw++)
                    {
                        _stage[dw][dh].type = _stage[dw][dh + 1].type;
                    }
                }
                h--;
                deleteLineCount++;
                _minoDefaultSpeed -= 0.001f;
            }
        }

        RefreshStage();
        AddScore(deleteLineCount);
    }

    private void RefreshStage()
    {
        // タイプを参照して
        // 

        for (int h = 0; h < StageHeight; h++)
        {
            for (int w = 0; w < StageWidth; w++)
            {

                switch (_stage[w][h].type)
                {
                    case Mino.MinoType.Empty:
                        _stage[w][h].obj.gameObject.SetActive(false);
                        break;
                    case Mino.MinoType.Wall:
                        _stage[w][h].obj.material.color = new Color32(0, 0, 0, 0);
                        _stage[w][h].obj.gameObject.SetActive(true);
                        break;
                    case Mino.MinoType.I:
                    case Mino.MinoType.J:
                    case Mino.MinoType.L:
                    case Mino.MinoType.S:
                    case Mino.MinoType.Z:
                    case Mino.MinoType.O:
                    case Mino.MinoType.T:
                        _stage[w][h].obj.material.color = _mino[(int)_stage[w][h].type].minoColor;
                        _stage[w][h].obj.gameObject.SetActive(true);
                        break;
                    default:
                        Debug.Log("例外のタイプが使われている");
                        break;
                }
               
            }
        }
    }

    private void AddScore(int deleteLineCount)
    {
        switch (deleteLineCount)
        {
            case 1:
                _score += 40;
                break;
            case 2:
                _score += 100;
                break;
            case 3:
                _score += 300;
                break;
            case 4:
                _score += 1200;
                break;
        }

        _scoreText.text = string.Format("SCORE:{0:00000}",_score);

    }

    private void GameOverEffect()
    {
        for (int h = 0; h < StageHeight; h++)
        {
            for (int w = 0; w < StageWidth; w++)
            {
                if (_stage[w][h].type != Mino.MinoType.Wall &&
                    _stage[w][h].type != Mino.MinoType.Empty)
                {
                    _stage[w][h].obj.material.color = new Color32(255,255,255,255);
                }
            }
        }
    }

    private bool IsWall(int w, int h)
    {
        bool ret = false;
        // bit演算0x00000 ->
        ret |= (w == 0);
        ret |= (h == 0);
        ret |= (w == StageWidth - 1);
        return ret;
    }


    private bool CountTimer()
    {
        _countStart += Time.deltaTime;
        if (_countStart >= 1f)
        {
            _countNumber++;
            _countStart = 0;
        }

        switch (_countNumber)
        {
            case 0:
                _startText.text = "Ready?";
                break;
            case 1:
                _startText.text = "3";
                // _startText.text = _countNumber.ToString();
                break;
            case 2:
                _startText.text = "2";
                // _startText.text = _countNumber.ToString();
                break;
            case 3:
                _startText.text = "1";
                // _startText.text = _countNumber.ToString();
                break;
            case 4:
                _startText.text = "GO!";
                break;
            case 5:
                _startText.text = "";
                _startFlag = true ;
                break;
            default:
                break;
        }

        if (_countNumber == 5) { return true; }
        return false;
    }
}
