# coding: utf-8

require 'io/console'
require 'ansi/code'
require 'g'

require 'forwardable'

module BlockGame

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
          when :quit
            IO.console.print "bye\r\n"
            exit
          else
            if counter >= 5
              process
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
        next_board = display_board
        loop do
          IO.console.print ANSI::Code.move 0, 0
          IO.console.print next_board
          IO.console.print ANSI::Code.move WIN_ROWS - 1, 0
          IO.console.print "h: left, l: right, q: quit\r\n"
          next_board = display_board
          sleep SLEEP_RATE
          IO.console.print ANSI::Code.cls
        end
      end
    end

    attr_reader :board, :x, :y, :current_unit

    BOARD_ROWS = 40
    BOARD_COLS = 100

    WIN_ROWS = 45
    WIN_COLS = 100

    BLOCK_SIZE = 10

    SLEEP_RATE = 0.05

    def initialize(messanger)
      @msg = messanger
      create_board(BOARD_ROWS, BOARD_COLS)
      @level = 0
      reset_poses
      IO.console.winsize = [WIN_ROWS, WIN_COLS]
    end

    def reset_poses
      @bar_x = (BOARD_COLS / 2) - (bar_size / 2)
      @bar_y = BOARD_ROWS - 5
      @ball_x = rand(BOARD_COLS)
      @ball_y = @bar_y - 20
      @move_x = [1, -1].sample
      @move_y = 1
    end

    def create_board(rows, cols)
      @board = Array.new(rows) { Array.new(cols) { ' ' } }
      10.times do |y|
        10.times do |x|
          c = rand(ANSI::Code.colors.size)
          c = 2 if c == 0
          10.times do |i|
            @board[y][(10 * x) + i] = c
          end
        end
      end
    end

    def color(str, color)
      ANSI::Code.__send__(color) { str }
    end

    def display_board
      _board = Marshal.load(Marshal.dump(board))

      set_bar(_board)
      set_ball(_board)

      _board = _board.map {|row| '|  ' + row.join('') + '  |' }
      _board.each do |row|
        row.gsub!(/\d/) {|i| color '=', ANSI::Code.colors[i.to_i] }
      end
      width = _board.last.size

      result = ''
      result << _board.join("\r\n")
      result << "\r\n"
      result << '-' * width
      result << "\r\n"
      result
    end

    def set_bar(_board)
      bar_size.times do |i|
        _board[@bar_y][@bar_x + i] = '-'
      end
    end

    def set_ball(_board)
      _board[@ball_y][@ball_x] = '@'
    end

    def next_ball
      reset_poses
    end

    def delete_block
      start_point = (@ball_x / 10) * 10
      10.times do |i|
        board[@ball_y - 1][start_point + i] = ' '
      end
    end

    def process
      ret = check(0, @move_y, :ball)
      case ret
      when :block
        delete_block
        bound_y
      when :over
        next_ball
        return
      end
      ret = check(@move_x, 0, :ball)
      case ret
      when :bound_board
        bound_x
      when :bound_bar
        bound_x
        bound_y
      end
      @ball_x += @move_x
      @ball_y += @move_y
    end

    def bound_x
      @move_x = -@move_x
    end

    def bound_y
      @move_y = -@move_y
    end

    MAX_BAR_SIZE = 5

    def bar_size
      [MAX_BAR_SIZE - @level, 1].max
    end

    def board_cols
      BOARD_COLS
    end

    def board_rows
      BOARD_ROWS
    end

    def move_right
      return if check(1, 0, :bar)
      @bar_x += 1
    end

    def move_left
      return if check(-1, 0, :bar)
      @bar_x -= 1
    end

    def check(move_x = 0, move_y = 0, type = :bar)
      case type
      when :bar
        next_x = @bar_x + move_x
        if next_x < 0 || next_x + bar_size > board_cols
          return :over
        end
      when :ball
        next_x = @ball_x + move_x
        if next_x < 0 || next_x > board_cols
          return :bound_board
        end
        next_y = @ball_y + move_y
        if next_y == @bar_y
          if (@bar_x..(@bar_x + bar_size)).include?(next_x)
            return :bound_bar
          end
        end
        if board[next_y][next_x] != ' '
          return :block
        end
        if next_y > board_rows
          return :over
        end
        if next_y < 0
          return :bound_bar
        end
      end
      nil
    end

    def start!
      @input  = input_proc(@msg)
      @output = output_proc(@msg)
      @controller = controller_proc(@msg)
      [@output, @controller, @input].map(&:join)
    end
  end
end

BlockGame.run
