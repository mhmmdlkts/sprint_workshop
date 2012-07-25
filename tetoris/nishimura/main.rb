# coding: utf-8

require 'io/console'
require 'ansi/code'

require 'forwardable'

module Tetoris

  def self.run
    @msg = Messanger.new
    core = Core.new(@msg)
    core.start!
  end

  class Messanger
    extend Forwardable
    def_delegator :@commands, :shift

    def initialize
      @commands = []
    end

    def push(cmd)
      return if @commands.last == cmd
      @commands.push(cmd)
    end
  end

  class Core
    def input_proc(msg)
      Thread.start(msg) do |msg|
        IO.console.raw do |io|
          io.chars do |ch|
            case ch
              when 'h' then msg.push(:left)
              when 'l' then msg.push(:right)
              when 'j' then msg.push(:down)
              when 'k' then msg.push(:rotate)
              when 'q' then msg.push(:quit)
              else
                # NOP
            end
          end
        end
      end
    end

    def controller_proc(msg)
      Thread.start(msg) do |msg|
        counter = 0
        loop do
          case msg.shift
          when :left   then move_left
          when :right  then move_right
          when :down   then move_down
          when :rotate then rotate
          when :quit
            IO.console.print "bye\r\n"
            exit
          else
            if counter >= 10
              move_down
              counter = 0
            end
          end
          counter += 1
          sleep SLEEP_RATE
        end
      end
    end

    def output_proc(msg)
      Thread.start(msg) do |msg|
        loop do
          IO.console.print ANSI::Code.move 0, 0
          IO.console.print display_board
          IO.console.print ANSI::Code.move WIN_ROWS - 2, 0
          IO.console.print "Position: #{x} x #{y}\r\n"
          IO.console.print "h: left, l: right, j: down, k: rotate\r\n"
          sleep SLEEP_RATE
          IO.console.print ANSI::Code.cls
        end
      end
    end

    attr_reader :board, :x, :y, :current_unit

    BOARD_ROWS = 20
    BOARD_COLS = 10

    WIN_ROWS = 25
    WIN_COLS = 30

    MINO_SIZE = 4

    SLEEP_RATE = 0.05

    def initialize(messanger)
      @msg = messanger
      create_board(BOARD_ROWS, BOARD_COLS)
      reset_poses
      IO.console.winsize = [30, 30]

      @current_unit = Unit.new
    end

    def reset_poses
      @x = (BOARD_COLS / 2) - (MINO_SIZE / 2)
      @y = 0
    end

    def create_board(rows, cols)
      @board = Array.new(rows) { Array.new(cols) { ' ' } }
    end

    def color(str, color)
      ANSI::Code.__send__(color) { str }
    end

    def display_board
      _board = Marshal.load(Marshal.dump(board))

      set_unit(_board)

      _board = _board.map {|row| '|  ' + row.join('  ') + '  |' }
      _board.each do |row|
        row.gsub!(/\d/) {|i| color '#', ANSI::Code.colors[i.to_i] }
      end
      width = _board.first.size

      result = ''
      result << _board.join("\r\n")
      result << "\r\n"
      result << '-' * width
      result << "\r\n"
      result
    end

    def set_unit(_board)
      current_unit.current.each_slice(4).with_index do |vals, y_offset|
        vals.each_with_index do |val, x_offset|
          next if val == 0
          _board[y + y_offset][x + x_offset] = current_unit.id
        end
      end
    end

    def next_unit
      current_unit.next!
      reset_poses
    end

    def move_down
      unless check(0, 1)
        set_unit(board)
        clear_line
        next_unit
        return
      end
      @y += 1
    end

    def clear_line
      board.each_with_index do |row, y|
        next if row.any? {|col| col == ' ' }
        row.size.times do |x|
          (y - 1).times do |_y|
            board[y - _y][x] = board[y - _y - 1][x]
          end
          board[0][x] = ' '
        end
      end
    end

    def move_right
      return unless check(1, 0)
      @x += 1
    end

    def move_left
      return unless check(-1, 0)
      @x -= 1
    end

    def rotate
      current_unit.rotate
    end

    def check(move_x = 0, move_y = 0)
      current_unit.current.each_slice(4).with_index do |vals, y_offset|
        vals.each_with_index do |val, x_offset|
          next if val == 0
          next_y = y + y_offset + move_y
          return false if next_y >= board.size
          next_x = x + x_offset + move_x
          return false if next_x < 0
          return false if board[next_y][next_x] != ' '
        end
      end
      true
    end

    def start!
      @input  = input_proc(@msg)
      @output = output_proc(@msg)
      @controller = controller_proc(@msg)
      [@output, @controller, @input].map(&:join)
    end
  end

  class Unit
    SHAPES = [
      [1,
        [
          [0, 1, 1, 0,
           0, 1, 1, 0],

          [0, 1, 1, 0,
           0, 1, 1, 0],

          [0, 1, 1, 0,
           0, 1, 1, 0],

          [0, 1, 1, 0,
           0, 1, 1, 0],
        ]
      ],
      [2,
        [
          [1, 1, 1, 1,
           0, 0, 0, 0],

          [1, 0, 0, 0,
           1, 0, 0, 0,
           1, 0, 0, 0,
           1, 0, 0, 0],

          [1, 1, 1, 1,
           0, 0, 0, 0],

          [1, 0, 0, 0,
           1, 0, 0, 0,
           1, 0, 0, 0,
           1, 0, 0, 0],
        ]
      ],
      [3,
        [
          [0, 1, 1, 0,
           1, 1, 0, 0],

          [1, 0, 0, 0,
           1, 1, 0, 0,
           0, 1, 0, 0],

          [0, 1, 1, 0,
           1, 1, 0, 0],

          [1, 0, 0, 0,
           1, 1, 0, 0,
           0, 1, 0, 0],
        ]
      ],
      [4,
        [
          [1, 1, 0, 0,
           0, 1, 1, 0],

          [0, 1, 0, 0,
           1, 1, 0, 0,
           1, 0, 0, 0],

          [1, 1, 0, 0,
           0, 1, 1, 0],

          [0, 1, 0, 0,
           1, 1, 0, 0,
           1, 0, 0, 0],
        ]
      ],
      [5,
        [
          [1, 1, 1, 0,
           0, 0, 1, 0],

          [0, 1, 0, 0,
           0, 1, 0, 0,
           1, 1, 0, 0],

          [1, 0, 0, 0,
           1, 1, 1, 0],

          [1, 1, 0, 0,
           1, 0, 0, 0,
           1, 0, 0, 0],
        ]
      ],
      [6,
        [
          [1, 1, 1, 0,
           1, 0, 0, 0],

          [1, 1, 0, 0,
           0, 1, 0, 0,
           0, 1, 0, 0],

          [0, 0, 1, 0,
           1, 1, 1, 0],

          [1, 0, 0, 0,
           1, 0, 0, 0,
           1, 1, 0, 0],
        ]
      ],
      [7,
        [
          [0, 1, 0, 0,
           1, 1, 1, 0],

          [1, 0, 0, 0,
           1, 1, 0, 0,
           1, 0, 0, 0],

          [1, 1, 1, 0,
           0, 1, 0, 0],

          [0, 1, 0, 0,
           1, 1, 0, 0,
           0, 1, 0, 0],
        ]
      ],
    ].freeze

    attr_reader :id, :current

    def initialize
      next!
    end

    def next!
      @id, (@current, _, _, _) = *SHAPES.sample
      @direction = 0
    end

    def rotate
      @direction = (@direction + 1) % 4
      @current = SHAPES.assoc(id).last[@direction]
    end
  end
end

Tetoris.run

